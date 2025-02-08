using AAK.Podio.Models;
using AktBob.Database.Contracts;
using AktBob.Deskpro.Contracts;
using AktBob.JobHandlers.Handlers;
using AktBob.Podio.Contracts;
using AktBob.Shared;
using AktBob.Shared.Contracts;
using Ardalis.GuardClauses;
using Hangfire;
using MassTransit;
using MassTransit.Mediator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AktBob.PodioHookProcessor.UseCases;
internal class CreateToSharepointQueueItemJobHandler(ILogger<CreateToSharepointQueueItemJobHandler> logger, IConfiguration configuration, IServiceScopeFactory serviceScopeFactory) : IJobHandler<CreateToSharepointQueueItemJob>
{
    private readonly ILogger<CreateToSharepointQueueItemJobHandler> _logger = logger;
    private readonly IConfiguration _configuration = configuration;
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    public async Task Handle(CreateToSharepointQueueItemJob job, CancellationToken cancellationToken = default)
    {
        var tenancyName = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("UiPath:TenancyName"));
        var uiPathQueueName = Guard.Against.NullOrEmpty(_configuration.GetValue<string>($"CreateToSharepointQueueItemJobHandler:UiPathQueueName:{tenancyName}"));
        var podioAppId = Guard.Against.Null(_configuration.GetValue<int?>("Podio:AppId"));
        var podioFields = Guard.Against.Null(Guard.Against.NullOrEmpty(_configuration.GetSection("Podio:Fields").GetChildren().ToDictionary(x => int.Parse(x.Key), x => x.Get<(int AppId, string Label)>())));
        var podioFieldCaseNumber = Guard.Against.Null(podioFields.FirstOrDefault(x => x.Value.AppId == podioAppId && x.Value.Label == "CaseNumber"));
        Guard.Against.Null(podioFieldCaseNumber.Value);

        using var scope = _serviceScopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        // Get metadata from Podio
        var getPodioItemQuery = new GetItemQuery(podioAppId, job.PodioItemId);
        var getPodioItemQueryResult = await mediator.SendRequest(getPodioItemQuery, cancellationToken);

        if (!getPodioItemQueryResult.IsSuccess)
        {
            _logger.LogError("Could not get item {itemId} from Podio", job.PodioItemId);
            return;
        }

        var caseNumber = getPodioItemQueryResult.Value.GetField(podioFieldCaseNumber.Key)?.GetValues<FieldValueText>()?.Value ?? string.Empty;
        if (string.IsNullOrEmpty(caseNumber))
        {
            _logger.LogError("Could not get case number field value from Podio Item {itemId}", job.PodioItemId);
            return;
        }

        // Find Deskpro ticket from PodioItemId
        var getTicketQuery = new GetTicketsQuery(null, job.PodioItemId, null);
        var getTicketResult = await mediator.SendRequest(getTicketQuery, cancellationToken);

        if (!getTicketResult.IsSuccess)
        {
            _logger.LogError("Error getting ticket from database by PodioItemId {podioItemId}.", job.PodioItemId);
            return;
        }

        if (getTicketResult.Value.Count() < 1)
        {
            _logger.LogError("No tickets found in database for PodioItemId {podioItemId}.", job.PodioItemId);
            return;
        }

        if (getTicketResult.Value.Count() > 1)
        {
            _logger.LogWarning("{count} Deskpro tickets found in database for PodioItemId {podioItemId}. Only processing the first.", getTicketResult.Value.Count(), job.PodioItemId);
        }

        var ticket = getTicketResult.Value.First();

        var caseSharepointFolderName = ticket.Cases?.FirstOrDefault(x => x.PodioItemId == job.PodioItemId)?.SharepointFolderName;
        if (caseSharepointFolderName == null)
        {
            _logger.LogError("Case related to PodioItemId {id}: SharepointFolderName is null or empty", job.PodioItemId);
            return;
        }

        var filArkivCaseId = ticket.Cases?.FirstOrDefault(c => c.PodioItemId == job.PodioItemId)?.FilArkivCaseId;
        if (filArkivCaseId == null)
        {
            _logger.LogError("FilArkivCaseId not found for PodioItemId {podioItemId}", job.PodioItemId);
            return;
        }

        var getDeskproTicketQuery = new GetDeskproTicketByIdQuery(ticket.DeskproId);
        var getDeskproTicketResult = await mediator.SendRequest(getDeskproTicketQuery, cancellationToken);

        if (!getDeskproTicketResult.IsSuccess)
        {
            _logger.LogError("Error getting ticket {id} from Deskpro", ticket.DeskproId);
            return;
        }


        // Get Deskpro agent
        (string Name, string Email) agent = new(string.Empty, string.Empty);

        if (getDeskproTicketResult.Value.Agent is not null && getDeskproTicketResult.Value.Agent.Id > 0)
        {
            agent = await GetDeskproAgent(mediator, getDeskproTicketResult.Value.Agent.Id, cancellationToken);
        }
        else
        {
            _logger.LogWarning($"Deskpro ticket {ticket.DeskproId} has no agents assigned");
        }

        var payload = new
        {
            SagsNummer = caseNumber,
            Email = agent.Email,
            Navn = agent.Name,
            PodioID = job.PodioItemId,
            DeskproID = getDeskproTicketResult.Value.Id,
            Titel = getDeskproTicketResult.Value.Subject,
            FilarkivCaseID = filArkivCaseId?.ToString() ?? string.Empty,
            Overmappenavn = ticket.SharepointFolderName,
            Undermappenavn = caseSharepointFolderName
        };

        BackgroundJob.Enqueue<CreateUiPathQueueItem>(x => x.Run(uiPathQueueName, job.PodioItemId.ToString(), payload, CancellationToken.None));
    }

    private async Task<(string Name, string Email)> GetDeskproAgent(IMediator mediator, int agentId, CancellationToken cancellationToken = default)
    {
        var getAgentQuery = new GetDeskproPersonQuery(agentId);
        var getAgentResult = await mediator.SendRequest(getAgentQuery, cancellationToken);

        if (getAgentResult.IsSuccess && getAgentResult.Value.IsAgent)
        {
            return (getAgentResult.Value.FullName, getAgentResult.Value.Email);
        }
        else
        {
            _logger.LogWarning($"Unable to get agent from Deskpro, agent id {agentId}");
        }

        return (string.Empty, string.Empty);
    }
}