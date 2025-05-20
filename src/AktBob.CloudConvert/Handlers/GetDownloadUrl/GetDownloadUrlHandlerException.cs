namespace AktBob.CloudConvert.Handlers.GetDownloadUrl;
internal class GetDownloadUrlHandlerException : IGetDownloadUrlHandler
{
    private readonly IGetDownloadUrlHandler _inner;
    private readonly ILogger<GetDownloadUrlHandler> _logger;

    public GetDownloadUrlHandlerException(IGetDownloadUrlHandler inner, ILogger<GetDownloadUrlHandler> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<ErrorOr<string>> Handle(Guid jobId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _inner.Handle(jobId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Error in {name}", nameof(GetDownloadUrlHandler));
            return Error.Failure("GenerateTasksHandler.Failure", ex.Message);
        }
    }
}
