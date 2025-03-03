using AktBob.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.Deskpro.JobHandlers;

internal record InvokeWebhookJob(string WebhookId, object Payload);

internal class InvokeWebhook(IServiceScopeFactory serviceScopeFactory) : IJobHandler<InvokeWebhookJob>
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    public async Task Handle(InvokeWebhookJob job, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IInvokeDeskproWebhookHandler>();
        await handler.Handle(job.WebhookId, job.Payload, cancellationToken);
    }
}
