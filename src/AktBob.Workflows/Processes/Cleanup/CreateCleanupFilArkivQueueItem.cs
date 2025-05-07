using AktBob.Database.Contracts;
using AktBob.OpenOrchestrator.Contracts;
using AktBob.Shared.Extensions;

namespace AktBob.Workflows.Processes.Cleanup;
internal class CreateCleanupFilArkivQueueItem : IJobHandler<CreateCleanupFilArkivQueueItemJob>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<CreateCleanupFilArkivQueueItem> _logger;
    private readonly IConfiguration _configuration;

    public CreateCleanupFilArkivQueueItem(IServiceScopeFactory serviceScopeFactory, ILogger<CreateCleanupFilArkivQueueItem> logger, IConfiguration configuration)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task Handle(CreateCleanupFilArkivQueueItemJob job, CancellationToken cancellationToken = default)
    {
        // Variables
        var openOrchestratorQueueName = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("DispatchCleanupJobsHandler:QueueNames:CleanUpFilArkivQueueName"));

        // Services
        using var scope = _serviceScopeFactory.CreateScope();
        var ticketRepository = scope.ServiceProvider.GetRequiredService<ITicketRepository>();
        var openOrchestrator = scope.ServiceProvider.GetRequiredService<IOpenOrchestratorModule>();

        // Get FilArkiv caseId from database
        var tickets = await ticketRepository.GetAll(DeskproId: job.DeskproTicketId, null, null);
        if (tickets is null || tickets.Count() == 0)
        {
            throw new BusinessException($"Deskpro ticket {job.DeskproTicketId} not found in database");
        }

        foreach (var ticket in tickets)
        {
            if (ticket.Cases.Count == 0)
            {
                _logger.LogInformation("No cases found in database for Deskpro ticket {id}", ticket.DeskproId);
                continue;
            }

            foreach (var @case in ticket.Cases)
            {
                if (@case.FilArkivCaseId is null)
                {
                    _logger.LogInformation("FilArkivCaseId is null for case {caseId} DeskproTicketId {id}", @case.CaseNumber, job.DeskproTicketId);
                    continue;
                }

                var payload = new
                {
                    @case.FilArkivCaseId
                };

                var command = new CreateQueueItemCommand(openOrchestratorQueueName, $"Deskpro {job.DeskproTicketId} {@case.CaseNumber}", payload.ToJson());
                openOrchestrator.CreateQueueItem(command);
            }
        }
    }
}
