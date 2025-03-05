using AAK.Deskpro;

namespace AktBob.Deskpro.Handlers;
internal class InvokeWebhookHandler(IDeskproClient deskproClient) : IInvokeWebhookHandler
{
    private readonly IDeskproClient _deskproClient = deskproClient;

    public async Task Handle(string webhookId, string payload, CancellationToken cancellationToken)
    {
        await _deskproClient.PostWebhook(webhookId, payload, cancellationToken);
    }
}