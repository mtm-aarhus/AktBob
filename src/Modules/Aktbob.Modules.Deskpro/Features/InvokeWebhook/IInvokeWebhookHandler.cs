namespace Aktbob.Modules.Deskpro.Features.InvokeWebhook;
internal interface IInvokeWebhookHandler
{
    Task<ErrorOr<Success>> Handle(string webhookId, string payload, CancellationToken cancellationToken);
}