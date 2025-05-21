namespace AktBob.Deskpro.Handlers.DownloadMessageAttachment;
internal class DownloadMessageAttachmentHandlerException : IDownloadMessageAttachmentHandler
{
    private readonly IDownloadMessageAttachmentHandler _inner;
    private readonly ILogger<DownloadMessageAttachmentHandler> _logger;

    public DownloadMessageAttachmentHandlerException(IDownloadMessageAttachmentHandler inner, ILogger<DownloadMessageAttachmentHandler> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<ErrorOr<Stream>> Handle(string downloadUrl, CancellationToken cancellationToken)
    {
        try
        {
            return await _inner.Handle(downloadUrl, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            if (ex.StatusCode is System.Net.HttpStatusCode.NotFound)
            {
                return Error.NotFound("DownloadMessageAttachmentHandler.NotFound", $"Attachment not found ({downloadUrl})");
            }

            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(DownloadMessageAttachmentHandler));
            return Error.Failure("DownloadMessageAttachmentHandler.Failure", ex.Message);
        }
    }
}
