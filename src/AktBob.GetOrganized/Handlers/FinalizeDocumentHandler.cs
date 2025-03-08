using AAK.GetOrganized;
using AktBob.GetOrganized.Contracts;

namespace AktBob.GetOrganized.Handlers;
internal class FinalizeDocumentHandler(IGetOrganizedClient getOrganizedClient) : IFinalizeDocumentHandler
{
    private readonly IGetOrganizedClient _getOrganizedClient = getOrganizedClient;

    public async Task Handle(int documentId, bool shouldCloseOpenTasks = false, CancellationToken cancellationToken = default)
    {
        await _getOrganizedClient.FinalizeDocument(documentId, shouldCloseOpenTasks, cancellationToken);
    }
}
