using Microsoft.Extensions.Logging;

namespace AktBob.GetOrganized.Handlers.FinalizeDocument;

internal class FinalizeDocumentHandlerException(IFinalizeDocumentHandler next, ILogger<FinalizeDocumentHandler> logger)
    : IFinalizeDocumentHandler
{
    public async Task Handle(int documentId, bool shouldCloseOpenTasks = false, CancellationToken cancellationToken = default)
    {
        try
        {
            await next.Handle(documentId, shouldCloseOpenTasks, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in {name}", nameof(FinalizeDocumentHandler));
        }
    }
}