using AAK.GetOrganized;
using AktBob.GetOrganized.Contracts;

namespace AktBob.GetOrganized.Handlers;
internal class FinalizeDocumentHandler(IGetOrganizedClient getOrganizedClient) : IFinalizeDocumentHandler
{
    private readonly IGetOrganizedClient _getOrganizedClient = getOrganizedClient;

    public async Task Handle(FinalizeDocumentCommand command, CancellationToken cancellationToken = default)
    {
        await _getOrganizedClient.FinalizeDocument(command.DocumentId, command.ShouldCloseOpenTasks, cancellationToken);
    }
}
