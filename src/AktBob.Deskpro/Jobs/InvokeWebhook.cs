using AktBob.Shared;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Text.Json;

namespace AktBob.Deskpro.Jobs;

internal record InvokeWebhookJob(string WebhookId, string Payload);

internal class InvokeWebhook(IServiceScopeFactory serviceScopeFactory) : IJobHandler<InvokeWebhookJob>
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    public async Task Handle(InvokeWebhookJob job, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IInvokeWebhookHandler>();

        var bytes = Convert.FromBase64String(job.Payload);
        var decodedPayload = Encoding.UTF8.GetString(bytes);

        await handler.Handle(job.WebhookId, decodedPayload, cancellationToken);
    }
}
