namespace AktBob.Deskpro.Contracts;
internal interface IInvokeDeskproWebhookHandler
{
    Task Handle(string webhookId, object payload, CancellationToken cancellationToken);
}