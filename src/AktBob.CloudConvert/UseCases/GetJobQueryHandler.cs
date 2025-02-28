using AktBob.Shared;

namespace AktBob.CloudConvert.UseCases;
internal class GetJobQueryHandler(ICloudConvertClient cloudConvertClient,
                                ILogger<GetJobQueryHandler> logger,
                                ITimeProvider timeProvider,
                                IQueryDispatcher queryDispatcher) : IQueryHandler<GetJobQuery, Result<byte[]>>
{
    private readonly ICloudConvertClient _cloudConvertClient = cloudConvertClient;
    private readonly ILogger<GetJobQueryHandler> _logger = logger;
    private readonly ITimeProvider _timeProvider = timeProvider;
    private readonly IQueryDispatcher _queryDispatcher = queryDispatcher;

    public async Task<Result<byte[]>> Handle(GetJobQuery request, CancellationToken cancellationToken)
    {
        var finished = false;

        while (!finished)
        {
            await _timeProvider.Delay(2000, cancellationToken);

            var getJobResult = await _cloudConvertClient.GetJob(request.JobId, cancellationToken);
            if (!getJobResult.IsSuccess)
            {
                // TODO
                finished = true;
            }

            if (getJobResult.Value!.Data.Status == "error")
            {
                _logger.LogError("Cloud Convert job error {id}", request.JobId);
                finished = true;
            }

            var file = getJobResult.Value?.Data.Tasks.Where(x => x.Operation == "export/url").FirstOrDefault()?.Result?.Files?.FirstOrDefault(x => !string.IsNullOrEmpty(x.Url));

            if (getJobResult.Value!.Data.Status == "finished" && !string.IsNullOrEmpty(file?.Url))
            {
                var getFileQuery = new GetFileQuery(file.Url);
                var getFileResult = await _queryDispatcher.Dispatch(getFileQuery, cancellationToken);

                if (!getFileResult.IsSuccess)
                {
                    // TODO
                    _logger.LogError("Error downloading {url}, Cloud Convert {id}", file.Url, request.JobId);
                    finished = true;
                }
                else
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        getFileResult.Value.Stream?.CopyTo(memoryStream);
                        finished = true;

                        _logger.LogInformation("Cloud Convert job {id} finished", request.JobId);
                        return memoryStream.ToArray();
                    }
                }
            }
        }

        return Result.Error();
    }
}