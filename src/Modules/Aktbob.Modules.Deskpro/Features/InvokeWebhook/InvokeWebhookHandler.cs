using AAK.Deskpro;

namespace Aktbob.Modules.Deskpro.Features.InvokeWebhook;
internal class InvokeWebhookHandler(IDeskproClient deskproClient) : IInvokeWebhookHandler
{
    private readonly IDeskproClient _deskproClient = deskproClient;

    public async Task<ErrorOr<Success>> Handle(string webhookId, string payload, CancellationToken cancellationToken)
    {
        await _deskproClient.PostWebhook(webhookId, payload, cancellationToken);
        return Result.Success;
    }
}