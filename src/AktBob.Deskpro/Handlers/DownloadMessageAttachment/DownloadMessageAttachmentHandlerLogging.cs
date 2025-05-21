using AktBob.Shared.Extensions;

namespace AktBob.Deskpro.Handlers.DownloadMessageAttachment;
internal class DownloadMessageAttachmentHandlerLogging : IDownloadMessageAttachmentHandler
{
    private readonly IDownloadMessageAttachmentHandler _inner;
    private readonly ILogger<DownloadMessageAttachmentHandler> _logger;

    public DownloadMessageAttachmentHandlerLogging(IDownloadMessageAttachmentHandler inner, ILogger<DownloadMessageAttachmentHandler> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<ErrorOr<Stream>> Handle(string downloadUrl, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Downloading Deskpro message attachment. Url = {url}", downloadUrl);

        var result = await _inner.Handle(downloadUrl, cancellationToken);

        result.Switch(
            _ => _logger.LogInformation("Deskpro message attachment downloaded ({url})", downloadUrl),
            errors => _logger.LogWarning("{name}: {errors}", nameof(DownloadMessageAttachment), errors.ToCommaDelimitedString()));

        return result;
    }
}
