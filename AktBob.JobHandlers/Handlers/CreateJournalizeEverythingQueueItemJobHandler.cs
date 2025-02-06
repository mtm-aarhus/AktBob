using AktBob.Database.Contracts;
using AktBob.Deskpro.Contracts;
using AktBob.OpenOrchestrator.Contracts;
using AktBob.Shared;
using AktBob.Shared.Contracts;
using AktBob.UiPath.Contracts;
using Ardalis.GuardClauses;
using MassTransit;
using MassTransit.Mediator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AktBob.PodioHookProcessor.UseCases;
internal class CreateJournalizeEverythingQueueItemJobHandler(IServiceScopeFactory serviceScopeFactory, IConfiguration configuration, ILogger<CreateJournalizeEverythingQueueItemJobHandler> logger) : IJobHandler<CreateJournalizeEverythingQueueItemJob>
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<CreateJournalizeEverythingQueueItemJobHandler> _logger = logger;
    private readonly string _configurationObjectName = "CreateJournalizeEverythingQueueItemJobHandler";

    public async Task Handle(CreateJournalizeEverythingQueueItemJob job, CancellationToken cancellationToken = default)
    {
        // UiPath variables
        var uiPathTenancyName = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("UiPath:TenancyName"));
        var uiPathQueueName = Guard.Against.NullOrEmpty(_configuration.GetValue<string>($"{_configurationObjectName}:UiPathQueueName:{uiPathTenancyName}"));

        // OpenOrchestrator variables
        var openOrchestratorQueueName = Guard.Against.NullOrEmpty(_configuration.GetValue<string>($"{_configurationObjectName}:OpenOrchestratorQueueName"));
        var useOpenOrchestrator = _configuration.GetValue<bool>($"{_configurationObjectName}:UseOpenOrchestrator");

        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            // GET DATA FROM API DATABASE
            var getDataFromApiDatabaseQuery = new GetTicketsQuery(job.DeskproId, null, null);
            var getDataFromApiDatabaseResult = await mediator.SendRequest(getDataFromApiDatabaseQuery, cancellationToken);

            if (getDataFromApiDatabaseResult.IsSuccess)
            {
                if (getDataFromApiDatabaseResult.Value.Count() < 1)
                {
                    _logger.LogError("No Deskpro tickets found for id {id}.", job.DeskproId);
                    return;
                }

                if (getDataFromApiDatabaseResult.Value.Count() > 1)
                {
                    _logger.LogWarning("{count} Deskpro tickets found for id {id}. Only processing the first.", getDataFromApiDatabaseResult.Value.Count(), job.DeskproId);
                }

                var ticket = getDataFromApiDatabaseResult.Value.FirstOrDefault();

                if (ticket is null)
                {
                    _logger.LogError("Ticket related to Deskpro Id {id} not found in database", job.DeskproId);
                    return;
                }

                if (string.IsNullOrEmpty(ticket.CaseNumber))
                {
                    _logger.LogError("GO Aktindsigtssagsnummer not registered for Deskpro Id {id}", job.DeskproId);
                    return;
                }



                // GET DATA FROM DESKPRO
                var getDeskproTicketQuery = new GetDeskproTicketByIdQuery(ticket.DeskproId);
                var getDeskproTicketQueryResult = await mediator.SendRequest(getDeskproTicketQuery, cancellationToken);

                if (getDeskproTicketQueryResult.IsSuccess)
                {
                    var agentName = string.Empty;
                    var agentEmail = string.Empty;

                    // Skip if the Deskpro ticket has no assigned agent
                    if (getDeskproTicketQueryResult.Value.Agent is not null && getDeskproTicketQueryResult.Value.Agent.Id > 0)
                    {
                        // Get agent email address from Deskpro
                        var getAgentQuery = new GetDeskproPersonQuery(getDeskproTicketQueryResult.Value.Agent.Id!);
                        var getAgentResult = await mediator.SendRequest(getAgentQuery, cancellationToken);

                        if (getAgentResult.IsSuccess && getAgentResult.Value.IsAgent)
                        {
                            agentName = getAgentResult.Value.FullName;
                            agentEmail = getAgentResult.Value.Email;
                        }
                    }



                    // CREATE QUEUE ITEM FOR UIPATH/OPENORCHESTRATOR

                    if (useOpenOrchestrator)
                    {
                        // Create OpenOrchestrator queue item
                        var data = new
                        {
                            Aktindsigtssag = ticket.CaseNumber,
                            Email = agentEmail,
                            Navn = agentName,
                            DeskproID = job.DeskproId,
                            Overmappenavn = ticket.SharepointFolderName
                        };

                        var command = new CreateQueueItemCommand(openOrchestratorQueueName, data, $"Deskpro ID {job.DeskproId}");
                        await mediator.Send(command, cancellationToken);
                    }
                    else
                    {
                        // Create UiPath queue item
                        var uiPathQueueItemContent = new
                        {
                            Aktindsigtssag = ticket.CaseNumber,
                            Email = agentEmail,
                            Navn = agentName,
                            DeskproID = job.DeskproId,
                            Overmappenavn = ticket.SharepointFolderName
                        };

                        var addUiPathQueueItemCommand = new AddQueueItemCommand(uiPathQueueName, $"DeskproId {job.DeskproId.ToString()}", uiPathQueueItemContent);
                        await mediator.Send(addUiPathQueueItemCommand, cancellationToken);
                    }
                }
            }
        }
    }
}
