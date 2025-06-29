namespace Aktbob.Modules.Deskpro.Features.InvokeWebhook;
public interface IInvokeWebhookHandler
{
    Task<ErrorOr<Success>> Handle(string webhookId, string payload, CancellationToken cancellationToken);
}