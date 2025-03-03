using AktBob.OpenOrchestrator.Contracts;
using AktBob.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.OpenOrchestrator;
internal class CreateQueueItem(IServiceScopeFactory serviceScopeFactory) : IJobHandler<CreateOpenOrchestratorQueueItemJob>
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    public async Task Handle(CreateOpenOrchestratorQueueItemJob job, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICreateOpenOrchestratorQueueItemHandler>();
        await handler.Handle(job.QueueName, job.Payload, job.Reference, cancellationToken);
    }
}