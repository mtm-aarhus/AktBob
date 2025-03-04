using AAK.GetOrganized;
using AAK.GetOrganized.UploadDocument;
using AktBob.GetOrganized.Contracts;
using Ardalis.Result;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AktBob.GetOrganized.Handlers;
internal class UploadGetOrganizedDocumenHandler(ILogger<UploadGetOrganizedDocumenHandler> logger, IConfiguration configuration, IGetOrganizedClient getOrganizedClient) : IUploadDocumentHandler
{
    private readonly ILogger<UploadGetOrganizedDocumenHandler> _logger = logger;
    private readonly IConfiguration _configuration = configuration;
    private readonly IGetOrganizedClient _getOrganizedClient = getOrganizedClient;

    public async Task<Result<int>> Handle(byte[] bytes, string caseNumber, string fileName, UploadDocumentMetadata metadata, bool overwrite, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Uploading document to GetOrganized (CaseNumber: {caseNumber}, FileName: '{filename}', file size (bytes): {filesize}) ...", caseNumber, fileName, bytes.Length);
            var listName = _configuration.GetValue<string>("GetOrganized:DefaultListName") ?? "Dokumenter";

            var result = await _getOrganizedClient.UploadDocument(bytes,
                                                                  caseNumber,
                                                                  listName,
                                                                  string.Empty,
                                                                  fileName,
                                                                  metadata,
                                                                  overwrite,
                                                                  cancellationToken);

            if (result is not null)
            {
                _logger.LogInformation("Document uploaded to GetOrganized (CaseNumber: {caseNumber}, FileName: '{filename}').", caseNumber, fileName);
                return result.DocumentId;
            }

            _logger.LogError("Error uploading document to GetOrganized (CaseNumber: {caseNumber}, FileName: '{filename}')", caseNumber, fileName);
            return Result.Error();
        }
        catch
        {
            _logger.LogError("Error uploading document to GetOrganized (CaseNumber: {caseNumber}, FileName: '{filename}')", caseNumber, fileName);
            return Result.Error();
        }
    }
}
