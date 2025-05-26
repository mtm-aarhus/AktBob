using Microsoft.Extensions.Logging;

namespace AktBob.GetOrganized.Handlers.FinalizeDocument;

internal class FinalizeDocumentHandlerException : IFinalizeDocumentHandler
{
    private readonly IFinalizeDocumentHandler _next;
    private readonly ILogger<FinalizeDocumentHandler> _logger;

    public FinalizeDocumentHandlerException(IFinalizeDocumentHandler next, ILogger<FinalizeDocumentHandler> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task Handle(int documentId, bool shouldCloseOpenTasks = false, CancellationToken cancellationToken = default)
    {
        try
        {
            await _next.Handle(documentId, shouldCloseOpenTasks, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(FinalizeDocumentHandler));
        }
    }
}