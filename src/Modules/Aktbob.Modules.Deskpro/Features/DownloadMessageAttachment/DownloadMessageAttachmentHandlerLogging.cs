using AktBob.Shared.Extensions;

namespace Aktbob.Modules.Deskpro.Features.DownloadMessageAttachment;
internal class DownloadMessageAttachmentHandlerLogging(
    IDownloadMessageAttachmentHandler inner,
    ILogger<DownloadMessageAttachmentHandler> logger) : IDownloadMessageAttachmentHandler
{
    private readonly IDownloadMessageAttachmentHandler _inner = inner;
    private readonly ILogger<DownloadMessageAttachmentHandler> _logger = logger;

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
