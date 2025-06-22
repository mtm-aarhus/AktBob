using AktBob.Shared.Contracts.Modules.FilArkiv.Contracts.DTOs;
using AktBob.Shared.Extensions;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace AktBob.Shared.ModuleClients.FilArkiv;

internal class FilArkivModuleClientLogging(IFilArkivModuleClient next, ILogger<FilArkivModuleClient> logger) : IFilArkivModuleClient
{
    public async Task<ErrorOr<IReadOnlyCollection<DocumentDto>>> GetDocumentsByCaseId(Guid caseId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting documents for case {caseId}", caseId);
        
        var result = await next.GetDocumentsByCaseId(caseId, cancellationToken);
        result.Switch(
            value => logger.LogInformation("Documents for case {caseId} retrieved successfully, count: {count}", caseId, value.Count),
            _ => result.LogResultErrors(logger));
        
        return result;
    }

    public async Task<ErrorOr<FileProcessStatusDto>> GetFileProcessStatus(Guid fileId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting file processing status for file {fileId}", fileId);
        
        var result = await next.GetFileProcessStatus(fileId, cancellationToken);
        result.Switch(
            _ => logger.LogInformation("Processing status for file {fileId} retrieved successfully", fileId),
            _ => result.LogResultErrors(logger));
        
        return result;
    }
}