using AAK.GetOrganized;

namespace AktBob.GetOrganized.Handlers.FinalizeDocument;
internal class FinalizeDocumentHandler(IGetOrganizedClient getOrganizedClient) : IFinalizeDocumentHandler
{
    public async Task Handle(int documentId, bool shouldCloseOpenTasks = false, CancellationToken cancellationToken = default)
    {
        await getOrganizedClient.FinalizeDocument(documentId, shouldCloseOpenTasks, cancellationToken);
    }
}
