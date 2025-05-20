using AktBob.Shared;

namespace AktBob.CloudConvert.Handlers.GetDownloadUrl;

internal class GetDownloadUrlHandler(ICloudConvertClient cloudConvertClient, ITimeProvider timeProvider) : IGetDownloadUrlHandler
{
    private readonly ICloudConvertClient _cloudConvertClient = cloudConvertClient;
    private readonly ITimeProvider _timeProvider = timeProvider;

    public async Task<ErrorOr<string>> Handle(Guid jobId, CancellationToken cancellationToken = default)
    {
        while (true)
        {
            var delay = TimeSpan.FromSeconds(2);
            await _timeProvider.Delay(delay, cancellationToken);

            var getJobResult = await _cloudConvertClient.GetJob(jobId, cancellationToken);
            if (getJobResult.IsError || getJobResult.Value.Data.Status == "error")
            {
                return getJobResult.Errors;
            }

            var file = getJobResult.Value?.Data.Tasks.Where(x => x.Operation == "export/url").FirstOrDefault()?.Result?.Files?.FirstOrDefault(x => !string.IsNullOrEmpty(x.Url));

            if (getJobResult.Value!.Data.Status == "finished" && !string.IsNullOrEmpty(file?.Url))
            {
                return file.Url;
            }
        }
    }
}