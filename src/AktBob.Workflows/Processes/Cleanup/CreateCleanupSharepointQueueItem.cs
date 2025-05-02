using AktBob.Database.Contracts;
using AktBob.OpenOrchestrator.Contracts;
using AktBob.Shared.Extensions;

namespace AktBob.Workflows.Processes.Cleanup;
internal class CreateCleanupSharepointQueueItem : IJobHandler<CreateCleanupSharepointQueueItemJob>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IConfiguration _configuration;

    public CreateCleanupSharepointQueueItem(IServiceScopeFactory serviceScopeFactory, IConfiguration configuration)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _configuration = configuration;
    }

    public async Task Handle(CreateCleanupSharepointQueueItemJob job, CancellationToken cancellationToken = default)
    {
        // Variables
        var openOrchestratorQueueName = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("DispatchCleanupJobsHandler:QueueNames:CleanUpSharepointQueueName"));

        // Services
        using var scope = _serviceScopeFactory.CreateScope();
        var ticketRepository = scope.ServiceProvider.GetRequiredService<ITicketRepository>();
        var openOrchestrator = scope.ServiceProvider.GetRequiredService<IOpenOrchestratorModule>();

        // Get Sharepoint folder name from database
        var ticket = await ticketRepository.GetByDeskproTicketId(job.DeskproTicketId);
        if (ticket == null)
        {
            throw new BusinessException($"Deskpro ticket {job.DeskproTicketId} not found in database,");
        }

        if (string.IsNullOrWhiteSpace(ticket.SharepointFolderName))
        {
            throw new BusinessException($"No Sharepoint folder name registered for Deskpro ticket {job.DeskproTicketId}.");
        }

        var payload = new
        {
            SharepointMappeNavn = ticket.SharepointFolderName
        };

        var command = new CreateQueueItemCommand(openOrchestratorQueueName, $"Deskpro {job.DeskproTicketId}", payload.ToJson());
        openOrchestrator.CreateQueueItem(command);
    }
}
