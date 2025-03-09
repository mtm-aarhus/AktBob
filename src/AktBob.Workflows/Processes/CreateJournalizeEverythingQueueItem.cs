using AktBob.Database.Contracts;
using AktBob.Deskpro.Contracts;
using AktBob.Deskpro.Contracts.DTOs;
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
        Guard.Against.NegativeOrZero(job.DeskproId);

        var scope = _serviceScopeFactory.CreateScope();

        // Services
        var deskpro = scope.ServiceProvider.GetRequiredService<IDeskproModule>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
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

        var agent = deskproTicketResult.Value.Agent?.Id != null
            ? await deskpro.GetPerson(deskproTicketResult.Value.Agent.Id, cancellationToken)
            : Result<PersonDto>.Error();

        // CREATE QUEUE ITEM
        if (useOpenOrchestrator)
        {
            // Create OpenOrchestrator queue item
            var payload = new
            {
                Aktindsigtssag = databaseTicket.CaseNumber,
                Email = agent.Value.Email,
                Navn = agent.Value.FullName,
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
                Email = agent.Value.Email,
                Navn = agent.Value.FullName,
                DeskproID = job.DeskproId,
                Overmappenavn = databaseTicket.SharepointFolderName
            };

            uiPath.CreateQueueItem(uiPathQueueName, $"Deskpro ID {job.DeskproId.ToString()}", payload.ToJson());
        }
    }
}
