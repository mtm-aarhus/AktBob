using AAK.GetOrganized;
using AktBob.GetOrganized.Contracts;
using MediatR;

namespace AktBob.GetOrganized.UseCases;
internal class RelateDocumentCommandHandler(IGetOrganizedClient getOrganizedClient) : IRequestHandler<RelateDocumentCommand>
{
    private readonly IGetOrganizedClient _getOrganizedClient = getOrganizedClient;

    public async Task Handle(RelateDocumentCommand request, CancellationToken cancellationToken)
    {
        if (request.ChildDocumentIds.Any())
        {
            await _getOrganizedClient.RelateDocuments(request.ParentDocumentId, request.ChildDocumentIds, request.RelationType, cancellationToken);
        }
    }
}
