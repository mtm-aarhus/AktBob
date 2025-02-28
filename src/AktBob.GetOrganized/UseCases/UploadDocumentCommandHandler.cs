using AAK.GetOrganized;
using AktBob.GetOrganized.Contracts;
using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AktBob.GetOrganized.UseCases;
internal class UploadDocumentCommandHandler(ILogger<UploadDocumentCommandHandler> logger, IConfiguration configuration, IGetOrganizedClient getOrganizedClient) : IRequestHandler<UploadDocumentCommand, Result<int>>
{
    private readonly ILogger<UploadDocumentCommandHandler> _logger = logger;
    private readonly IConfiguration _configuration = configuration;
    private readonly IGetOrganizedClient _getOrganizedClient = getOrganizedClient;

    public async Task<Result<int>> Handle(UploadDocumentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Uploading document to GetOrganized (CaseNumber: {caseNumber}, FileName: '{filename}', file size (bytes): {filesize}) ...", request.CaseNumber, request.FileName, request.Bytes.Length);
            var listName = _configuration.GetValue<string>("GetOrganized:DefaultListName") ?? "Dokumenter";

            var result = await _getOrganizedClient.UploadDocument(
                                request.Bytes,
                                request.CaseNumber,
                                listName,
                                string.Empty,
                                request.FileName,
                                request.Metadata,
                                request.Overwrite,
                                cancellationToken);

            if (result is not null)
            {
                _logger.LogInformation("Document uploaded to GetOrganized (CaseNumber: {caseNumber}, FileName: '{filename}').", request.CaseNumber, request.FileName);
                return result.DocumentId;
            }
            
            _logger.LogError("Error uploading document to GetOrganized (CaseNumber: {caseNumber}, FileName: '{filename}')", request.CaseNumber, request.FileName);
            return Result.Error();
        }
        catch
        {
            _logger.LogError("Error uploading document to GetOrganized (CaseNumber: {caseNumber}, FileName: '{filename}')", request.CaseNumber, request.FileName);
            return Result.Error();
        }
    }
}
