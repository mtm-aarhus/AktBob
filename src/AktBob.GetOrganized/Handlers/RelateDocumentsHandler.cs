using AAK.GetOrganized;
using AAK.GetOrganized.RelateDocuments;
using AktBob.GetOrganized.Contracts;

namespace AktBob.GetOrganized.Handlers;
internal class RelateDocumentsHandler(IGetOrganizedClient getOrganizedClient) : IRelateDocumentsHandler
{
    private readonly IGetOrganizedClient _getOrganizedClient = getOrganizedClient;

    public async Task Handle(RelateDocumentsCommand command, CancellationToken cancellationToken = default)
    {
        if (command.ChildrenDocumentsIds.Any())
        {
            await _getOrganizedClient.RelateDocuments(command.ParentDocumentId, command.ChildrenDocumentsIds, RelationType.Bilag, cancellationToken);
        }
    }
}
