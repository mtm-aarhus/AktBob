using AktBob.Shared.Extensions;

namespace AktBob.CloudConvert.Handlers.GetDownloadUrl;
internal class GetDownloadUrlHandlerLogging : IGetDownloadUrlHandler
{
    private readonly IGetDownloadUrlHandler _inner;
    private readonly ILogger<GetDownloadUrlHandler> _logger;

    public GetDownloadUrlHandlerLogging(IGetDownloadUrlHandler inner, ILogger<GetDownloadUrlHandler> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<ErrorOr<string>> Handle(Guid jobId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting download url for CloudConvert jobId {id}", jobId);

        var result = await _inner.Handle(jobId, cancellationToken);

        result.Switch(
            value => _logger.LogInformation("Download url for CloudConvert jobId {id}: {url}", jobId, value),
            errors => _logger.LogWarning("{name}: {error}", nameof(GetDownloadUrl), errors.ToCommaDelimitedString()));

        return result;
    }
}
