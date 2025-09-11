using Microsoft.Extensions.Logging;

namespace AktBob.GetOrganized.Handlers.RelateDocuments;

internal class RelateDocumentsHandlerLogging(IRelateDocumentsHandler next, ILogger<RelateDocumentsHandler> logger)
    : IRelateDocumentsHandler
{
    public async Task Handle(int parentDocumentId, int[] childrenDocumentsIds, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Relating GetOrganized documents. Parent: {parentId}. Children: {children}", parentDocumentId, childrenDocumentsIds);
        await next.Handle(parentDocumentId, childrenDocumentsIds, cancellationToken);
    }
}