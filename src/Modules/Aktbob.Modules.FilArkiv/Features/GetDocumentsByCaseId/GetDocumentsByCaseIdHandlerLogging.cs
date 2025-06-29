using AktBob.Shared.Contracts.Modules.FilArkiv.Contracts.DTOs;
using AktBob.Shared.Extensions;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace Aktbob.Modules.FilArkiv.Features.GetDocumentsByCaseId;

internal class GetDocumentsByCaseIdHandlerLogging(
    IGetDocumentsByCaseIdHandler next,
    ILogger<GetDocumentsByCaseIdHandler> logger)
    : IGetDocumentsByCaseIdHandler
{
    public async Task<ErrorOr<IReadOnlyCollection<DocumentDto>>> Handle(Guid caseId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting FilArkiv documents by case {caseId}", caseId);

        var result = await next.Handle(caseId, cancellationToken);
        
        result.Switch(
            _ => logger.LogInformation("{count} documents by case {caseId} retrieved", result.Value.Count, caseId),
            errors => logger.LogWarning("Error getting documents by case {caseId}: {error}", caseId, errors.ToCommaDelimitedString()));
        
        return result;
    }
}