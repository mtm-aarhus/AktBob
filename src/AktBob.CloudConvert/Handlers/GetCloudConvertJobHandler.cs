using AktBob.Shared;

namespace AktBob.CloudConvert.Handlers;
internal class GetCloudConvertJobHandler(ICloudConvertClient cloudConvertClient,
                                         ILogger<GetCloudConvertJobHandler> logger,
                                         ITimeProvider timeProvider,
                                         IGetCloudConvertFileHandler getCloudConvertFileHandler) : IGetCloudConvertJobHandler
{
    private readonly ICloudConvertClient _cloudConvertClient = cloudConvertClient;
    private readonly ILogger<GetCloudConvertJobHandler> _logger = logger;
    private readonly ITimeProvider _timeProvider = timeProvider;
    private readonly IGetCloudConvertFileHandler _getCloudConvertFileHandler = getCloudConvertFileHandler;

    public async Task<Result<byte[]>> Handle(Guid jobId, CancellationToken cancellationToken = default)
    {
        var finished = false;

        while (!finished)
        {
            var delay = TimeSpan.FromSeconds(2);
            await _timeProvider.Delay(delay, cancellationToken);

            var getJobResult = await _cloudConvertClient.GetJob(jobId, cancellationToken);
            if (!getJobResult.IsSuccess)
            {
                // TODO
                finished = true;
            }

            if (getJobResult.Value!.Data.Status == "error")
            {
                _logger.LogError("Cloud Convert job error {id}", jobId);
                finished = true;
            }

            var file = getJobResult.Value?.Data.Tasks.Where(x => x.Operation == "export/url").FirstOrDefault()?.Result?.Files?.FirstOrDefault(x => !string.IsNullOrEmpty(x.Url));

            if (getJobResult.Value!.Data.Status == "finished" && !string.IsNullOrEmpty(file?.Url))
            {
                var getFileResult = await _getCloudConvertFileHandler.Handle(file.Url, cancellationToken);
                if (!getFileResult.IsSuccess)
                {
                    // TODO
                    _logger.LogError("Error downloading {url}, Cloud Convert {id}", file.Url, jobId);
                    finished = true;
                }
                else
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        getFileResult.Value.Stream?.CopyTo(memoryStream);
                        finished = true;

                        _logger.LogInformation("Cloud Convert job {id} finished", jobId);
                        return memoryStream.ToArray();
                    }
                }
            }
        }

        return Result.Error();
    }
}