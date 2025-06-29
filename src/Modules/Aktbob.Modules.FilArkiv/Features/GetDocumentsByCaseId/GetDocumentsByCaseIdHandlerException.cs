using AktBob.Shared.Contracts.Modules.FilArkiv.Contracts.DTOs;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace Aktbob.Modules.FilArkiv.Features.GetDocumentsByCaseId;

internal class GetDocumentsByCaseIdHandlerException(
    IGetDocumentsByCaseIdHandler next,
    ILogger<GetDocumentsByCaseIdHandler> logger)
    : IGetDocumentsByCaseIdHandler
{
    public async Task<ErrorOr<IReadOnlyCollection<DocumentDto>>> Handle(Guid caseId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await next.Handle(caseId, cancellationToken);
        }
        catch (HttpRequestException ex)
        when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            logger.LogWarning("Documents for FilArkiv case {caseId} not found.", caseId);
            return Error.NotFound("FilArkiv.DocumentsNotFound", $"No documents found for case {caseId}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in {name}", nameof(GetDocumentsByCaseIdHandler));
            throw;
        }
    }
}