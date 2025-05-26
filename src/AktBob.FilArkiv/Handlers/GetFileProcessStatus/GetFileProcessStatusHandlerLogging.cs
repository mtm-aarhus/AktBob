using AktBob.FilArkiv.Contracts.DTOs;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace AktBob.FilArkiv.Handlers.GetFileProcessStatus;

internal class GetFileProcessStatusHandlerLogging : IGetFileProcessStatusHandler
{
    private readonly IGetFileProcessStatusHandler _next;
    private readonly ILogger<GetFileProcessStatusHandler> _logger;

    public GetFileProcessStatusHandlerLogging(IGetFileProcessStatusHandler next, ILogger<GetFileProcessStatusHandler> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task<ErrorOr<FileProcessStatusDto>> Handle(Guid fileId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting file process status for file {fileId}", fileId);

        var result = await _next.Handle(fileId, cancellationToken);

        result.Switch(
            _ => _logger.LogInformation("File process status for file {fileId} retrieved", fileId),
            errors => _logger.LogWarning("Error getting file process status for file {fileId}", fileId));

        return result;
    }
}