using AAK.GetOrganized;
using AktBob.GetOrganized.Contracts;
using MassTransit.Mediator;

namespace AktBob.GetOrganized;
public class RelateDocumentCommandHandler(IGetOrganizedClient getOrganizedClient) : MediatorRequestHandler<RelateDocumentCommand>
{
    private readonly IGetOrganizedClient _getOrganizedClient = getOrganizedClient;

    protected override async Task Handle(RelateDocumentCommand request, CancellationToken cancellationToken)
    {
        await _getOrganizedClient.RelateDocuments(request.ParentDocumentId, request.ChildDocumentIds, request.RelationType, cancellationToken);
    }
}
