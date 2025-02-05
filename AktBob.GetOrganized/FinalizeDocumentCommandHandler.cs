using AAK.GetOrganized;
using AktBob.GetOrganized.Contracts;
using MassTransit.Mediator;

namespace AktBob.GetOrganized;
public class FinalizeDocumentCommandHandler(IGetOrganizedClient getOrganizedClient) : MediatorRequestHandler<FinalizeDocumentCommand>
{
    private readonly IGetOrganizedClient _getOrganizedClient = getOrganizedClient;

    protected override async Task Handle(FinalizeDocumentCommand request, CancellationToken cancellationToken)
    {
        await _getOrganizedClient.FinalizeDocument(request.DocumentId, request.ShouldCloseOpenTasks, cancellationToken);
    }
}
