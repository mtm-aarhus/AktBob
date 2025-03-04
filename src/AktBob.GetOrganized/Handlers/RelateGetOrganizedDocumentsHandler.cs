using AAK.GetOrganized;
using AAK.GetOrganized.RelateDocuments;
using AktBob.GetOrganized.Contracts;

namespace AktBob.GetOrganized.Handlers;
internal class RelateGetOrganizedDocumentsHandler(IGetOrganizedClient getOrganizedClient) : IRelateDocumentsHandler
{
    private readonly IGetOrganizedClient _getOrganizedClient = getOrganizedClient;

    public async Task Handle(int parentDocumentId, int[] childDocumentIds, RelationType relationType = RelationType.Bilag, CancellationToken cancellationToken = default)
    {
        if (childDocumentIds.Any())
        {
            await _getOrganizedClient.RelateDocuments(parentDocumentId, childDocumentIds, relationType, cancellationToken);
        }
    }
}
