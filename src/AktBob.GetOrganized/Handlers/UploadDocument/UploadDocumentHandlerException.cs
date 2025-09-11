using AktBob.GetOrganized.Contracts;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace AktBob.GetOrganized.Handlers.UploadDocument;

internal class UploadDocumentHandlerException(IUploadDocumentHandler next, ILogger<UploadDocumentHandler> logger)
    : IUploadDocumentHandler
{
    public async Task<ErrorOr<int>> Handle(
        byte[] bytes,
        string caseNumber,
        string fileName,
        string customProperty,
        DateTime documentDate,
        UploadDocumentCategory category,
        bool overwriteExisting,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next.Handle(
                bytes,
                caseNumber,
                fileName,
                customProperty,
                documentDate,
                category,
                overwriteExisting,
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in {name}", nameof(UploadDocumentHandler));
            return Error.Failure("GetOrganized.UpdateCaseMetadataHandler", $"Failed to upload document {fileName} to case {caseNumber}.");
        }
    }
}