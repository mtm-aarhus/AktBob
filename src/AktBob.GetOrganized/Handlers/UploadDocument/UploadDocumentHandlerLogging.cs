using AktBob.GetOrganized.Contracts;
using AktBob.Shared.Extensions;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace  AktBob.GetOrganized.Handlers.UploadDocument;

internal class UploadDocumentHandlerLogging : IUploadDocumentHandler
{
    private readonly IUploadDocumentHandler _next;
    private readonly ILogger<UploadDocumentHandler> _logger;

    public UploadDocumentHandlerLogging(IUploadDocumentHandler next, ILogger<UploadDocumentHandler> logger)
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
        _logger.LogInformation("Uploading document to GetOrganized case. CaseId = {caseId}, Filename = {filename}", caseNumber, fileName);

        var result = await _next.Handle(
            bytes,
            caseNumber,
            fileName,
            customProperty,
            documentDate,
            category,
            overwriteExisting,
            cancellationToken);
        
        result.Switch(
            _ => _logger.LogInformation("Document uploaded to GetOrganized case {caseId}, DocumentId: {id}, Filename = {filename}", caseNumber, result.Value, fileName),
            errors => _logger.LogWarning("{name}: {errors}", nameof(UploadDocument), result.Errors.ToCommaDelimitedString()));

        return result;
    }
}