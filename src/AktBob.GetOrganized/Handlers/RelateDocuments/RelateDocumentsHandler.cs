using AAK.GetOrganized;
using AAK.GetOrganized.RelateDocuments;

namespace AktBob.GetOrganized.Handlers.RelateDocuments;
internal class RelateDocumentsHandler(IGetOrganizedClient getOrganizedClient) : IRelateDocumentsHandler
{
    public async Task Handle(int parentDocumentId, int[] childrenDocumentsIds, CancellationToken cancellationToken = default)
    {
        if (childrenDocumentsIds.Length != 0)
        {
            await getOrganizedClient.RelateDocuments(parentDocumentId, childrenDocumentsIds, RelationType.Bilag, cancellationToken);
        }
    }
}