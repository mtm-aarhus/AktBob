using AktBob.FilArkiv.Contracts.DTOs;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace AktBob.FilArkiv.Handlers.GetDocumentsByCaseId;

internal class GetDocumentsByCaseIdHandlerException : IGetDocumentsByCaseIdHandler
{
    private readonly IGetDocumentsByCaseIdHandler _next;
    private readonly ILogger<GetDocumentsByCaseIdHandler> _logger;

    public GetDocumentsByCaseIdHandlerException(IGetDocumentsByCaseIdHandler next, ILogger<GetDocumentsByCaseIdHandler> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task<ErrorOr<IReadOnlyCollection<DocumentDto>>> Handle(Guid caseId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _next.Handle(caseId, cancellationToken);
        }
        catch (HttpRequestException ex)
        when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Documents for FilArkiv case {caseId} not found.", caseId);
            return Error.NotFound("FilArkiv.DocumentsNotFound", $"No documents found for case {caseId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(GetDocumentsByCaseIdHandler));
            throw;
        }
    }
}