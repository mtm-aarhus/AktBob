namespace AktBob.CloudConvert.Handlers.DownloadFile;
internal class DownloadFileHandlerException : IDownloadFileHandler
{
    private readonly IDownloadFileHandler _inner;
    private readonly ILogger<DownloadFileHandler> _logger;

    public DownloadFileHandlerException(IDownloadFileHandler inner, ILogger<DownloadFileHandler> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<ErrorOr<byte[]>> Handle(string url, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _inner.Handle(url, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Error in {name}", nameof(DownloadFileHandler));
            return Error.Failure("CloudConvertDownloadFileHandler.Failure", ex.Message);
        }
    }
}
