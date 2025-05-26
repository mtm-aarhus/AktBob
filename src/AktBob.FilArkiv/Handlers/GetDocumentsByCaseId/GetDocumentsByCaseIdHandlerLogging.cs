using AktBob.FilArkiv.Contracts.DTOs;
using AktBob.Shared.Extensions;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace AktBob.FilArkiv.Handlers.GetDocumentsByCaseId;

internal class GetDocumentsByCaseIdHandlerLogging : IGetDocumentsByCaseIdHandler
{
    private readonly IGetDocumentsByCaseIdHandler _next;
    private readonly ILogger<GetDocumentsByCaseIdHandler> _logger;

    public GetDocumentsByCaseIdHandlerLogging(IGetDocumentsByCaseIdHandler next, ILogger<GetDocumentsByCaseIdHandler> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task<ErrorOr<IReadOnlyCollection<DocumentDto>>> Handle(Guid caseId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting FilArkiv documents by case {caseId}", caseId);

        var result = await _next.Handle(caseId, cancellationToken);
        
        result.Switch(
            _ => _logger.LogInformation("{count} documents by case {caseId} retrieved", result.Value.Count, caseId),
            errors => _logger.LogWarning("Error getting documents by case {caseId}: {error}", caseId, errors.ToCommaDelimitedString()));
        
        return result;
    }
}