using AktBob.GetOrganized.Contracts;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace AktBob.GetOrganized.Handlers.UploadDocument;

internal class UploadDocumentHandlerException : IUploadDocumentHandler
{
    private readonly IUploadDocumentHandler _next;
    private readonly ILogger<UploadDocumentHandler> _logger;

    public UploadDocumentHandlerException(IUploadDocumentHandler next, ILogger<UploadDocumentHandler> logger)
    {
        _next = next;
        _logger = logger;
    }

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
            return await _next.Handle(
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
            _logger.LogError(ex, "Error in {name}", nameof(GetCaseMetadata));
            return Error.Failure("GetOrganized.UpdateCaseMetadataHandler", $"Failed to upload document {fileName} to case {caseNumber}.");
        }
    }
}