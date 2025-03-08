using AktBob.Database.Contracts;
using AktBob.Deskpro.Contracts;
using AktBob.Workflows.Helpers;
using AktBob.OpenOrchestrator.Contracts;
using AktBob.Shared.Extensions;
using AktBob.Shared.Jobs;
using AktBob.UiPath.Contracts;

namespace AktBob.Workflows.Processes;
internal class CreateJournalizeEverythingQueueItem(IServiceScopeFactory serviceScopeFactory, IConfiguration configuration, ILogger<CreateJournalizeEverythingQueueItem> logger) : IJobHandler<CreateJournalizeEverythingQueueItemJob>
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<CreateJournalizeEverythingQueueItem> _logger = logger;
    private readonly string _configurationObjectName = "CreateJournalizeEverythingQueueItemJobHandler";

    public async Task Handle(CreateJournalizeEverythingQueueItemJob job, CancellationToken cancellationToken = default)
    {
        var scope = _serviceScopeFactory.CreateScope();

        // Services
        var deskpro = scope.ServiceProvider.GetRequiredService<IDeskproModule>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var deskproHelper = scope.ServiceProvider.GetRequiredService<DeskproHelper>();
        var openOrchestrator = scope.ServiceProvider.GetRequiredService<IOpenOrchestratorModule>();
        var uiPath = scope.ServiceProvider.GetRequiredService<IUiPathModule>();

        // UiPath variables
        var uiPathTenancyName = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("UiPath:TenancyName"));
        var uiPathQueueName = Guard.Against.NullOrEmpty(_configuration.GetValue<string>($"{_configurationObjectName}:UiPathQueueName:{uiPathTenancyName}"));

        // OpenOrchestrator variables
        var openOrchestratorQueueName = Guard.Against.NullOrEmpty(_configuration.GetValue<string>($"{_configurationObjectName}:OpenOrchestratorQueueName"));
        var useOpenOrchestrator = _configuration.GetValue<bool>($"{_configurationObjectName}:UseOpenOrchestrator");


        // Get ticket from repository
        var databaseTicket = await unitOfWork.Tickets.GetByDeskproTicketId(job.DeskproId);

        if (databaseTicket is null)
        {
            _logger.LogError("No Deskpro tickets found for id {id}.", job.DeskproId);
            return;
        }

        if (string.IsNullOrEmpty(databaseTicket.CaseNumber))
        {
            _logger.LogError("GO Aktindsigtssagsnummer not registered for Deskpro Id {id}", job.DeskproId);
            return;
        }


        // Get ticket from Deskpro
        var deskproTicketResult = await deskpro.GetTicket(job.DeskproId, cancellationToken);

        if (!deskproTicketResult.IsSuccess)
        {
            _logger.LogError("Error ticket {id} from Deskpro", job.DeskproId);
            return;
        }

        var agent = await deskproHelper.GetAgent(deskpro, deskproTicketResult.Value.Agent?.Id ?? 0, cancellationToken);

        // CREATE QUEUE ITEM
        if (useOpenOrchestrator)
        {
            // Create OpenOrchestrator queue item
            var payload = new
            {
                Aktindsigtssag = databaseTicket.CaseNumber,
                Email = agent.Email,
                Navn = agent.Name,
                DeskproID = job.DeskproId,
                Overmappenavn = databaseTicket.SharepointFolderName
            };

            var createOpenOrchestratorQueueItemCommand = new CreateQueueItemCommand(openOrchestratorQueueName, $"Deskpro ID {job.DeskproId}", payload.ToJson());
            openOrchestrator.CreateQueueItem(createOpenOrchestratorQueueItemCommand);
        }
        else
        {
            // Create UiPath queue item
            var payload = new
            {
                Aktindsigtssag = databaseTicket.CaseNumber,
                Email = agent.Email,
                Navn = agent.Name,
                DeskproID = job.DeskproId,
                Overmappenavn = databaseTicket.SharepointFolderName
            };

            uiPath.CreateQueueItem(uiPathQueueName, $"Deskpro ID {job.DeskproId.ToString()}", payload.ToJson());
        }
    }
}
