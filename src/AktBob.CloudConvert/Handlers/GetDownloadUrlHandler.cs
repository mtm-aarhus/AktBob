using AktBob.Shared;

namespace AktBob.CloudConvert.Handlers;
internal class GetDownloadUrlHandler(ICloudConvertClient cloudConvertClient,
                                         ILogger<GetDownloadUrlHandler> logger,
                                         ITimeProvider timeProvider) : IGettDownloadUrlHandler
{
    private readonly ICloudConvertClient _cloudConvertClient = cloudConvertClient;
    private readonly ILogger<GetDownloadUrlHandler> _logger = logger;
    private readonly ITimeProvider _timeProvider = timeProvider;

    public async Task<Result<string>> Handle(Guid jobId, CancellationToken cancellationToken = default)
    {
        while (true)
        {
            var delay = TimeSpan.FromSeconds(2);
            await _timeProvider.Delay(delay, cancellationToken);

            var getJobResult = await _cloudConvertClient.GetJob(jobId, cancellationToken);
            if (!getJobResult.IsSuccess)
            {
                return Result.Error($"Error getting status for Cloud Convert job {jobId}");
            }

            if (getJobResult.Value.Data.Status == "error")
            {
                return Result.Error($"Cloud Convert job {jobId} errored");
            }

            var file = getJobResult.Value?.Data.Tasks.Where(x => x.Operation == "export/url").FirstOrDefault()?.Result?.Files?.FirstOrDefault(x => !string.IsNullOrEmpty(x.Url));

            if (getJobResult.Value!.Data.Status == "finished" && !string.IsNullOrEmpty(file?.Url))
            {
                _logger.LogInformation("CloudConvert job {id} finished. Download url: {url}", jobId, file.Url);
                return file.Url;
            }
        }
    }
}