using Microsoft.Extensions.Logging;

namespace AktBob.GetOrganized.Handlers.FinalizeDocument;

internal class FinalizeDocumentHandlerLogging : IFinalizeDocumentHandler
{
    private readonly IFinalizeDocumentHandler _next;
    private readonly ILogger<FinalizeDocumentHandler> _logger;

    public FinalizeDocumentHandlerLogging(IFinalizeDocumentHandler next, ILogger<FinalizeDocumentHandler> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task Handle(int documentId, bool shouldCloseOpenTasks = false, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Finalizing GetOrganized document {documentId}", documentId);
        await _next.Handle(documentId, shouldCloseOpenTasks, cancellationToken);
    }
}