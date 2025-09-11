using Microsoft.Extensions.Logging;

namespace AktBob.GetOrganized.Handlers.RelateDocuments;

internal class RelateDocumentsHandlerException(IRelateDocumentsHandler next, ILogger<RelateDocumentsHandler> logger) : IRelateDocumentsHandler
{
    public async Task Handle(int parentDocumentId, int[] childrenDocumentsIds, CancellationToken cancellationToken = default)
    {
        try
        {
            await next.Handle(parentDocumentId, childrenDocumentsIds, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in {name}", nameof(RelateDocumentsHandler));
        }
    }
}