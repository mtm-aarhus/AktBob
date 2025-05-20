using AktBob.Shared.Extensions;

namespace AktBob.CloudConvert.Handlers.DownloadFile;
internal class DownloadFileHandlerLogging : IDownloadFileHandler
{
    private readonly IDownloadFileHandler _inner;
    private readonly ILogger<DownloadFileHandler> _logger;

    public DownloadFileHandlerLogging(IDownloadFileHandler inner, ILogger<DownloadFileHandler> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<ErrorOr<byte[]>> Handle(string url, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Downloading file {url}", url);

        var result = await _inner.Handle(url, cancellationToken);
        result.Switch(
            value => _logger.LogInformation("File downloaded {url}", url),
            errors => _logger.LogWarning("{name}: {error}", nameof(DownloadFile), result.Errors.ToCommaDelimitedString()));

        return result;
    }
}
