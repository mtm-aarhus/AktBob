using AktBob.Database.Contracts;
using AktBob.Deskpro.Contracts;
using AktBob.Deskpro.Contracts.DTOs;
using AktBob.OpenOrchestrator.Contracts;
using AktBob.Shared.Extensions;
using AktBob.Shared.Jobs;

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
        var deskpro = scope.ServiceProvider.GetRequiredServiceOrThrow<IDeskproModule>();
        var unitOfWork = scope.ServiceProvider.GetRequiredServiceOrThrow<IUnitOfWork>();
        var openOrchestrator = scope.ServiceProvider.GetRequiredServiceOrThrow<IOpenOrchestratorModule>();

        // OpenOrchestrator variables
        var openOrchestratorQueueName = Guard.Against.NullOrEmpty(_configuration.GetValue<string>($"{_configurationObjectName}:OpenOrchestratorQueueName"));

        // Begin
        var getDatabaseTicket = unitOfWork.Tickets.GetByDeskproTicketId(job.DeskproId);
        var getDeskproTicket = deskpro.GetTicket(job.DeskproId, cancellationToken);

        await Task.WhenAll([getDatabaseTicket, getDeskproTicket]);

        if (getDatabaseTicket.Result is null) throw new BusinessException("Unable to get ticket from database");
        if (!getDeskproTicket.Result.IsSuccess) throw new BusinessException("Unable to get ticket from Deskpro");

        if (string.IsNullOrEmpty(getDatabaseTicket.Result.CaseNumber))
        {
            _logger.LogWarning("GO Aktindsigtssagsnummer not registered for Deskpro Id {id}", job.DeskproId);
        }

        var agent = getDeskproTicket.Result.Value.Agent?.Id != null
            ? await deskpro.GetPerson(getDeskproTicket.Result.Value.Agent.Id, cancellationToken)
            : Result<PersonDto>.Error();

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
}