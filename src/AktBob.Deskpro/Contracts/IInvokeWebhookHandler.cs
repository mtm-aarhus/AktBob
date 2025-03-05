namespace AktBob.Deskpro.Contracts;
internal interface IInvokeWebhookHandler
{
    Task Handle(string webhookId, string payload, CancellationToken cancellationToken);
}