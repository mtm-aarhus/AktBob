using AAK.Deskpro;
using AktBob.Deskpro.Contracts;
using MassTransit.Mediator;

namespace AktBob.Deskpro.UseCases;
public class InvokeWebhookCommandHandler(IDeskproClient deskproClient) : MediatorRequestHandler<InvokeWebhookCommand>
{
    private readonly IDeskproClient _deskproClient = deskproClient;

    protected override async Task Handle(InvokeWebhookCommand request, CancellationToken cancellationToken)
    {
        await _deskproClient.PostWebhook(request.WebhookId, request.Payload, cancellationToken);
    }
}
