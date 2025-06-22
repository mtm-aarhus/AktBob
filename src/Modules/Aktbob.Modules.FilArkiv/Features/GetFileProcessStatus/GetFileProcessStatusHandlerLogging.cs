using AktBob.Shared.Contracts.Modules.FilArkiv.Contracts.DTOs;
using ErrorOr;

namespace Aktbob.Modules.FilArkiv.Features.GetFileProcessStatus;

internal class GetFileProcessStatusHandlerLogging(
    IGetFileProcessStatusHandler next,
    ILogger<GetFileProcessStatusHandler> logger)
    : IGetFileProcessStatusHandler
{
    public async Task<ErrorOr<FileProcessStatusDto>> Handle(Guid fileId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting file process status for file {fileId}", fileId);

        var result = await next.Handle(fileId, cancellationToken);

        result.Switch(
            _ => logger.LogInformation("File process status for file {fileId} retrieved", fileId),
            errors => logger.LogWarning("Error getting file process status for file {fileId}", fileId));

        return result;
    }
}