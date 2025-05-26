using System.Net;
using AktBob.FilArkiv.Contracts.DTOs;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace AktBob.FilArkiv.Handlers.GetFileProcessStatus;

internal class GetFileProcessStatusHandlerException : IGetFileProcessStatusHandler
{
    private readonly IGetFileProcessStatusHandler _next;
    private readonly ILogger<GetFileProcessStatusHandler> _logger;

    public GetFileProcessStatusHandlerException(IGetFileProcessStatusHandler next, ILogger<GetFileProcessStatusHandler> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task<ErrorOr<FileProcessStatusDto>> Handle(Guid fileId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _next.Handle(fileId, cancellationToken);
        }
        catch (HttpRequestException ex)
        when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return Error.NotFound("FilArkiv.FileProcessStatusNotFound", $"File process status for FilArkiv file {fileId} not found."); 
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(GetFileProcessStatus));
            throw;
        }
    }
}