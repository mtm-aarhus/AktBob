using AktBob.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.OpenOrchestrator;

internal record CreateQueueItemJob(string QueueName, string Reference, string Payload);

internal class CreateQueueItem(IServiceScopeFactory serviceScopeFactory) : IJobHandler<CreateQueueItemJob>
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    public async Task Handle(CreateQueueItemJob job, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICreateQueueItemHandler>();
        await handler.Handle(job.QueueName, job.Payload, job.Reference, cancellationToken);
    }
}