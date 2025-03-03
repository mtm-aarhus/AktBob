using AktBob.Shared;
using AktBob.UiPath.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.UiPath;
internal class CreateQueueItem(IServiceScopeFactory serviceScopeFactory) : IJobHandler<CreateUiPathQueueItemJob>
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    public async Task Handle(CreateUiPathQueueItemJob job, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICreateUiPathQueueItemHandler>();
        await handler.Handle(job.QueueName, job.Reference, job.Payload, cancellationToken);
    }
}
