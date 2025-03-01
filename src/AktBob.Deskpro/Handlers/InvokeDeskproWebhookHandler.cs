using AAK.Deskpro;

namespace AktBob.Deskpro.Handlers;
internal class InvokeDeskproWebhookHandler(IDeskproClient deskproClient) : IInvokeDeskproWebhookHandler
{
    private readonly IDeskproClient _deskproClient = deskproClient;

    public async Task Handle(string webhookId, object payload, CancellationToken cancellationToken)
    {
        await _deskproClient.PostWebhook(webhookId, payload, cancellationToken);
    }
}