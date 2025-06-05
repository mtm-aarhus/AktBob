using AktBob.Shared.Extensions;

namespace Aktbob.Modules.Deskpro.Features.InvokeWebhook;
internal class InvokeWebhookHandlerLogging(IInvokeWebhookHandler inner, ILogger<InvokeWebhookHandler> logger) : IInvokeWebhookHandler
{
    private readonly ILogger<InvokeWebhookHandler> _logger = logger;
    private readonly IInvokeWebhookHandler _inner = inner;

    public Task<ErrorOr<Success>> Handle(string webhookId, string payload, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Invoking Deskpro webhook {id} with payload {payload}", webhookId, payload);
        var result = _inner.Handle(webhookId, payload, cancellationToken);
        
        result.Switch(
            _ => _logger.LogInformation("Deskpro webhook {id} invoked with payload {payload}", webhookId, payload),
            errors => _logger.LogWarning("{name}: {errors}", nameof(InvokeWebhookHandler), errors.ToCommaDelimitedString()));

        return result;
    }
}
