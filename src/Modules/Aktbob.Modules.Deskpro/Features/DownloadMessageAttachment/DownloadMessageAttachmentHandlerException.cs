namespace Aktbob.Modules.Deskpro.Features.DownloadMessageAttachment;
internal class DownloadMessageAttachmentHandlerException(
    IDownloadMessageAttachmentHandler inner,
    ILogger<DownloadMessageAttachmentHandler> logger) : IDownloadMessageAttachmentHandler
{
    private readonly IDownloadMessageAttachmentHandler _inner = inner;
    private readonly ILogger<DownloadMessageAttachmentHandler> _logger = logger;

    public async Task<ErrorOr<Stream>> Handle(string downloadUrl, CancellationToken cancellationToken)
    {
        try
        {
            return await _inner.Handle(downloadUrl, cancellationToken);
        }
        catch (HttpRequestException ex)
        when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return Error.NotFound("DownloadMessageAttachmentHandler.NotFound", $"Attachment not found ({downloadUrl})");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(DownloadMessageAttachmentHandler));
            return Error.Failure("DownloadMessageAttachmentHandler.Failure", ex.Message);
        }
    }
}
