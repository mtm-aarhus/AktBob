using AktBob.Database.Contracts;
using AktBob.Deskpro.Contracts;
using AktBob.Shared;
using AktBob.Shared.Contracts;
using Ardalis.GuardClauses;
using Hangfire;
using MassTransit;
using MassTransit.Mediator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AktBob.JobHandlers.Handlers;
internal class CreateAfgørelsesskrivelseQueueItemJobHandler(IServiceScopeFactory serviceScopeFactory, ILogger<CreateAfgørelsesskrivelseQueueItemJobHandler> logger, IConfiguration configuration) : IJobHandler<CreateAfgørelsesskrivelseQueueItemJob>
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly ILogger<CreateAfgørelsesskrivelseQueueItemJobHandler> _logger = logger;
    private readonly IConfiguration _configuration = configuration;

    public async Task Handle(CreateAfgørelsesskrivelseQueueItemJob job, CancellationToken cancellationToken = default)
    {
        var openOrchestratorQueueName = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("CreateAfgørelsesskrivelseQueueItemJobHandler:OpenOrchestratorQueueName"));
        var deskproAfdelingFieldId = Guard.Against.Null(_configuration.GetValue<int>("CreateAfgørelsesskrivelseQueueItemJobHandler:AfdelingFieldId"));

        using var scope = _serviceScopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        // Get data from Deskpro
        var getDeskproTicketQuery = new GetDeskproTicketByIdQuery(job.DeskproId);
        var getDeskproTicketResult = await mediator.SendRequest(getDeskproTicketQuery, cancellationToken);

        if (!getDeskproTicketResult.IsSuccess)
        {
            _logger.LogError("Error getting Deskpro ticket {id}", job.DeskproId);
            return;
        }


        var deskproTicket = getDeskproTicketResult.Value;
        var afdeling = deskproTicket.Fields.FirstOrDefault(x => x.Id == deskproAfdelingFieldId)?.Values.FirstOrDefault();
        if (string.IsNullOrEmpty(afdeling))
        {
            _logger.LogError("Deskpro ticket {id} field 'Afdeling' is null or empty", job.DeskproId);
            return;
        }


        // Get person from Deskpro
        if (deskproTicket.Person == null)
        {
            _logger.LogError("Deskpro ticket {id}: person is null", job.DeskproId);
            return;
        }


        var getPersonQuery = new GetDeskproPersonQuery(deskproTicket.Person.Id);
        var getPersonResult = await mediator.SendRequest(getPersonQuery, cancellationToken);

        if (!getPersonResult.IsSuccess)
        {
            _logger.LogError("Error getting person {id} from Deskpro", deskproTicket.Person.Id);
            return;
        }

        var person = getPersonResult.Value;

        // Get agent from Deskpro
        if (deskproTicket.Agent == null)
        {
            _logger.LogError("Deskpro ticket {id}: agent is null", job.DeskproId);
            return;
        }

        var getAgentQuery = new GetDeskproPersonQuery(deskproTicket.Agent.Id);
        var getAgentResult = await mediator.SendRequest(getAgentQuery, cancellationToken);

        if (!getAgentResult.IsSuccess)
        {
            _logger.LogError("Error getting agent {id} from Deskpro", deskproTicket.Agent.Id);
            return;
        }

        var agent = getAgentResult.Value;


        // Get data from database
        var getDatabaseTicketQuery = new GetTicketsQuery(job.DeskproId, null, null);
        var getDatabaseTicketResult = await mediator.SendRequest(getDatabaseTicketQuery, cancellationToken);

        if (!getDatabaseTicketResult.IsSuccess || getDatabaseTicketResult.Value == null || !getDatabaseTicketResult.Value.Any())
        {
            _logger.LogError("Error ticket from database (DeskproId {id})", job.DeskproId);
            return;
        }


        var databaseTicket = getDatabaseTicketResult.Value.First();
        if (string.IsNullOrEmpty(databaseTicket.SharepointFolderName))
        {
            _logger.LogError("Sharepoint folder name is null or empty (database ticket id {id}", databaseTicket.Id);
            return;
        }


        // Create OpenOrchestrator queue item
        var payload = new
        {
            AnsøgerNavn = person.FullName,
            AnsøgerEmail = person.Email,
            Afdeling = afdeling,
            Aktindsigtsovermappe = databaseTicket.SharepointFolderName,
            SagsbehandlerEmail = agent.Email
        };

        BackgroundJob.Enqueue<CreateOpenOrchestratorQueueItem>(x => x.Run(openOrchestratorQueueName, $"DeskproID {job.DeskproId}", payload.ToJson(), CancellationToken.None));
    }
}
