using AAK.Deskpro;
using AktBob.Deskpro.Contracts;

namespace AktBob.Deskpro.UseCases;
internal class InvokeWebhookCommandHandler(IDeskproClient deskproClient) : IRequestHandler<InvokeWebhookCommand>
{
    private readonly IDeskproClient _deskproClient = deskproClient;

    public async Task Handle(InvokeWebhookCommand request, CancellationToken cancellationToken)
    {
        await _deskproClient.PostWebhook(request.WebhookId, request.Payload, cancellationToken);
    }
}