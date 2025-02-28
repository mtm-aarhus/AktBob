using AktBob.Database.Contracts;
using AktBob.Deskpro.Contracts;
using AktBob.Shared.Contracts;

namespace AktBob.JobHandlers.Handlers;
internal class CreateJournalizeEverythingQueueItemJobHandler(IServiceScopeFactory serviceScopeFactory, IConfiguration configuration, ILogger<CreateJournalizeEverythingQueueItemJobHandler> logger) : IJobHandler<CreateJournalizeEverythingQueueItemJob>
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<CreateJournalizeEverythingQueueItemJobHandler> _logger = logger;
    private readonly string _configurationObjectName = "CreateJournalizeEverythingQueueItemJobHandler";

    public async Task Handle(CreateJournalizeEverythingQueueItemJob job, CancellationToken cancellationToken = default)
    {
        var scope = _serviceScopeFactory.CreateScope();
        var queryDispatcher = scope.ServiceProvider.GetRequiredService<IQueryDispatcher>();

        // UiPath variables
        var uiPathTenancyName = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("UiPath:TenancyName"));
        var uiPathQueueName = Guard.Against.NullOrEmpty(_configuration.GetValue<string>($"{_configurationObjectName}:UiPathQueueName:{uiPathTenancyName}"));

        // OpenOrchestrator variables
        var openOrchestratorQueueName = Guard.Against.NullOrEmpty(_configuration.GetValue<string>($"{_configurationObjectName}:OpenOrchestratorQueueName"));
        var useOpenOrchestrator = _configuration.GetValue<bool>($"{_configurationObjectName}:UseOpenOrchestrator");


        // GET DATA FROM API DATABASE
        var getTicketQuery = new GetTicketsQuery(job.DeskproId, null, null);
        var getTicketResult = await queryDispatcher.Dispatch(getTicketQuery, cancellationToken);

        if (!getTicketResult.IsSuccess)
        {
            _logger.LogError("Error getting ticket data from database for id {id}", job.DeskproId);
            return;
        }

        if (getTicketResult.Value.Count() < 1)
        {
            _logger.LogError("No Deskpro tickets found for id {id}.", job.DeskproId);
            return;
        }

        if (getTicketResult.Value.Count() > 1)
        {
            _logger.LogWarning("{count} Deskpro tickets found for id {id}. Only processing the first.", getTicketResult.Value.Count(), job.DeskproId);
        }

        var ticket = getTicketResult.Value.First();

        if (string.IsNullOrEmpty(ticket.CaseNumber))
        {
            _logger.LogError("GO Aktindsigtssagsnummer not registered for Deskpro Id {id}", job.DeskproId);
            return;
        }


        // GET DATA FROM DESKPRO
        var getDeskproTicketQuery = new GetDeskproTicketByIdQuery(ticket.DeskproId);
        var getDeskproTicketQueryResult = await queryDispatcher.Dispatch(getDeskproTicketQuery, cancellationToken);

        if (!getDeskproTicketQueryResult.IsSuccess)
        {
            _logger.LogError("Error ticket {id} from Deskpro", ticket.DeskproId);
            return;
        }

        var agentName = string.Empty;
        var agentEmail = string.Empty;

        // Skip if the Deskpro ticket has no assigned agent
        if (getDeskproTicketQueryResult.Value.Agent is not null && getDeskproTicketQueryResult.Value.Agent.Id > 0)
        {
            // Get agent email address from Deskpro
            var getAgentQuery = new GetDeskproPersonQuery(getDeskproTicketQueryResult.Value.Agent.Id!);
            var getAgentResult = await queryDispatcher.Dispatch(getAgentQuery, cancellationToken);

            if (getAgentResult.IsSuccess && getAgentResult.Value.IsAgent)
            {
                agentName = getAgentResult.Value.FullName;
                agentEmail = getAgentResult.Value.Email;
            }
        }



        // CREATE QUEUE ITEM

        if (useOpenOrchestrator)
        {
            // Create OpenOrchestrator queue item
            var payload = new
            {
                Aktindsigtssag = ticket.CaseNumber,
                Email = agentEmail,
                Navn = agentName,
                DeskproID = job.DeskproId,
                Overmappenavn = ticket.SharepointFolderName
            };

            BackgroundJob.Enqueue<CreateOpenOrchestratorQueueItem>(x => x.Run(openOrchestratorQueueName, $"Deskpro ID {job.DeskproId}", payload.ToJson(), CancellationToken.None));
        }
        else
        {
            // Create UiPath queue item
            var payload = new
            {
                Aktindsigtssag = ticket.CaseNumber,
                Email = agentEmail,
                Navn = agentName,
                DeskproID = job.DeskproId,
                Overmappenavn = ticket.SharepointFolderName
            };

            BackgroundJob.Enqueue<CreateUiPathQueueItem>(x => x.Run(uiPathQueueName, $"Deskpro ID {job.DeskproId.ToString()}", payload.ToJson(), CancellationToken.None));
        }
    }
}
