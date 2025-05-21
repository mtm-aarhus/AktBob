using AktBob.OpenOrchestrator.Contracts;
using AktBob.Shared.Extensions;
using AktBob.Shared.Jobs;

namespace AktBob.Workflows.Processes;
internal class CreateCloseNovaCaseQueueItem : IJobHandler<CreateCloseNovaCaseQueueItemJob>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;

    public CreateCloseNovaCaseQueueItem(IServiceScopeFactory scopeFactory, IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
    }

    public Task Handle(CreateCloseNovaCaseQueueItemJob job, CancellationToken cancellationToken = default)
    {
        var queueName = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("CreateCloseNovaCaseJobHandler:OpenOrchestratorQueueName"));

        using var scope = _scopeFactory.CreateScope();
        var openOrchestrator = scope.ServiceProvider.GetRequiredService<IOpenOrchestratorModule>();

        var payload = new
        {
            DeskProID = job.TicketId
        };

        var command = new CreateQueueItemCommand(queueName, $"Deskpro {job.TicketId}", payload.ToJson());
        openOrchestrator.CreateQueueItem(command);

        return Task.CompletedTask;
    }
}
