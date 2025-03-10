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


        // Begin
        var getDatabaseTicket = unitOfWork.Tickets.GetByDeskproTicketId(job.DeskproId);
        var getDeskproTicket = deskpro.GetTicket(job.DeskproId, cancellationToken);

        Task.WaitAll([getDatabaseTicket, getDeskproTicket]);

        if (getDatabaseTicket.Result is null
            || !getDeskproTicket.Result.IsSuccess)
        {
            _logger.LogCritical("Failed with {job}", job);
            return;
        }

        if (string.IsNullOrEmpty(getDatabaseTicket.Result.CaseNumber))
        {
            _logger.LogWarning("GO Aktindsigtssagsnummer not registered for Deskpro Id {id}", job.DeskproId);
        }

        var agent = getDeskproTicket.Result.Value.Agent?.Id != null
            ? await deskpro.GetPerson(getDeskproTicket.Result.Value.Agent.Id, cancellationToken)
            : Result<PersonDto>.Error();

        // Create queue item
        if (useOpenOrchestrator)
        {
            // Create OpenOrchestrator queue item
            var payload = new
            {
                Aktindsigtssag = getDatabaseTicket.Result.CaseNumber,
                Email = agent.Value.Email,
                Navn = agent.Value.FullName,
                DeskproID = job.DeskproId,
                Overmappenavn = getDatabaseTicket.Result.SharepointFolderName
            };

            var createOpenOrchestratorQueueItemCommand = new CreateQueueItemCommand(openOrchestratorQueueName, $"Deskpro ID {job.DeskproId}", payload.ToJson());
            openOrchestrator.CreateQueueItem(createOpenOrchestratorQueueItemCommand);
        }
        else
        {
            // Create UiPath queue item
            var payload = new
            {
                Aktindsigtssag = getDatabaseTicket.Result.CaseNumber,
                Email = agent.Value.Email,
                Navn = agent.Value.FullName,
                DeskproID = job.DeskproId,
                Overmappenavn = getDatabaseTicket.Result.SharepointFolderName
            };

            uiPath.CreateQueueItem(uiPathQueueName, $"Deskpro ID {job.DeskproId.ToString()}", payload.ToJson());
        }
    }
}
