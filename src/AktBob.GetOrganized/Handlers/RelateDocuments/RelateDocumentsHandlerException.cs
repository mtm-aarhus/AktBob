using Microsoft.Extensions.Logging;

namespace AktBob.GetOrganized.Handlers.RelateDocuments;

internal class RelateDocumentsHandlerException : IRelateDocumentsHandler
{
    private readonly IRelateDocumentsHandler _next;
    private readonly ILogger<RelateDocumentsHandler> _logger;

    public RelateDocumentsHandlerException(IRelateDocumentsHandler next, ILogger<RelateDocumentsHandler> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task Handle(int parentDocumentId, int[] childrenDocumentsIds, CancellationToken cancellationToken = default)
    {
        try
        {
            await _next.Handle(parentDocumentId, childrenDocumentsIds, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(RelateDocumentsHandler));
        }
    }
}