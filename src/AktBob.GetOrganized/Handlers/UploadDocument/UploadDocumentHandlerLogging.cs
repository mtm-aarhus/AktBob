using AktBob.GetOrganized.Contracts;
using AktBob.Shared.Extensions;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace  AktBob.GetOrganized.Handlers.UploadDocument;

internal class UploadDocumentHandlerLogging(IUploadDocumentHandler next, ILogger<UploadDocumentHandler> logger) : IUploadDocumentHandler
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
        logger.LogInformation("Uploading document to GetOrganized case. CaseId = {caseId}, Filename = {filename}", caseNumber, fileName);

        var result = await next.Handle(
            bytes,
            caseNumber,
            fileName,
            customProperty,
            documentDate,
            category,
            overwriteExisting,
            cancellationToken);
        
        result.Switch(
            _ => logger.LogInformation("Document uploaded to GetOrganized case {caseId}, DocumentId: {id}, Filename = {filename}", caseNumber, result.Value, fileName),
            errors => logger.LogWarning("{name}: {errors}", nameof(UploadDocumentHandler), result.Errors.ToCommaDelimitedString()));

        return result;
    }
}