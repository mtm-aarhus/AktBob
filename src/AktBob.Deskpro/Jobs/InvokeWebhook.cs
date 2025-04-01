using AktBob.Shared;
using AktBob.Shared.Exceptions;
using AktBob.Shared.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace AktBob.Deskpro.Jobs;

internal record InvokeWebhookJob(string WebhookId, string Payload);

internal class InvokeWebhook(IServiceScopeFactory serviceScopeFactory) : IJobHandler<InvokeWebhookJob>
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    public async Task Handle(InvokeWebhookJob job, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredServiceOrThrow<IInvokeWebhookHandler>();

        try
        {
            var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
            var bytes = Convert.FromBase64String(job.Payload);
            var decodedPayload = encoding.GetString(bytes);

            await handler.Handle(job.WebhookId, decodedPayload, cancellationToken);
        }
        catch (DecoderFallbackException ex)
        {
            throw new BusinessException($"Encoded payload not valid: {ex}");
        }
    }
}
