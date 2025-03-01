namespace AktBob.Deskpro.Contracts;
public interface IInvokeDeskproWebhookHandler
{
    Task Handle(string webhookId, object payload, CancellationToken cancellationToken);
}