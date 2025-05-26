using Microsoft.Extensions.Logging;

namespace AktBob.GetOrganized.Handlers.RelateDocuments;

internal class RelateDocumentsHandlerLogging : IRelateDocumentsHandler
{
    private readonly IRelateDocumentsHandler _next;
    private readonly ILogger<RelateDocumentsHandler> _logger;

    public RelateDocumentsHandlerLogging(IRelateDocumentsHandler next, ILogger<RelateDocumentsHandler> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task Handle(int parentDocumentId, int[] childrenDocumentsIds, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Relating GetOrganized documents. Parent: {parentId}. Children: {children}", parentDocumentId, childrenDocumentsIds);
        await _next.Handle(parentDocumentId, childrenDocumentsIds, cancellationToken);
    }
}