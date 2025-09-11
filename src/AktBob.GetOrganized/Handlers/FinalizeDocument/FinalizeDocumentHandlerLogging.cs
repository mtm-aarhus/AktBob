using Microsoft.Extensions.Logging;

namespace AktBob.GetOrganized.Handlers.FinalizeDocument;

internal class FinalizeDocumentHandlerLogging(IFinalizeDocumentHandler next, ILogger<FinalizeDocumentHandler> logger) : IFinalizeDocumentHandler
{
    public async Task Handle(int documentId, bool shouldCloseOpenTasks = false, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Finalizing GetOrganized document {documentId}", documentId);
        await next.Handle(documentId, shouldCloseOpenTasks, cancellationToken);
    }
}