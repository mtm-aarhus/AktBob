using System.Net;
using AktBob.Shared.Contracts.Modules.FilArkiv.Contracts.DTOs;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace Aktbob.Modules.FilArkiv.Features.GetFileProcessStatus;

internal class GetFileProcessStatusHandlerException(
    IGetFileProcessStatusHandler next,
    ILogger<GetFileProcessStatusHandler> logger)
    : IGetFileProcessStatusHandler
{
    public async Task<ErrorOr<FileProcessStatusDto>> Handle(Guid fileId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await next.Handle(fileId, cancellationToken);
        }
        catch (HttpRequestException ex)
        when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return Error.NotFound("FilArkiv.FileProcessStatusNotFound", $"File process status for FilArkiv file {fileId} not found."); 
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in {name}", nameof(GetFileProcessStatus));
            throw;
        }
    }
}