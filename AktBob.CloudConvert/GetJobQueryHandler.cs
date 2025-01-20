using AktBob.CloudConvert.Contracts;
using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AktBob.CloudConvert;
internal class GetJobQueryHandler(ICloudConvertClient cloudConvertClient, ILogger<GetJobQueryHandler> logger, IMediator mediator) : IRequestHandler<GetJobQuery, Result<byte[]>>
{
    private readonly ICloudConvertClient _cloudConvertClient = cloudConvertClient;
    private readonly ILogger<GetJobQueryHandler> _logger = logger;
    private readonly IMediator _mediator = mediator;

    public async Task<Result<byte[]>> Handle(GetJobQuery request, CancellationToken cancellationToken)
    {
        var finished = false;

        while (!finished)
        {
            await Task.Delay(5000);

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
                var getFileResult = await _mediator.Send(getFileQuery, cancellationToken);

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