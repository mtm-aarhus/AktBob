namespace AktBob.Deskpro.Contracts;
internal interface IInvokeWebhookHandler
{
    Task Handle(string webhookId, object payload, CancellationToken cancellationToken);
}