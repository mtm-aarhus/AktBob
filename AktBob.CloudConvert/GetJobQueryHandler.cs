using AktBob.CloudConvert.Contracts;
using AktBob.CloudConvert.Contracts.DTOs;
using Ardalis.Result;
using MediatR;

namespace AktBob.CloudConvert;
internal class GetJobQueryHandler(ICloudConvertClient cloudConvertClient) : IRequestHandler<GetJobQuery, Result<JobDto>>
{
    private readonly ICloudConvertClient _cloudConvertClient = cloudConvertClient;

    public async Task<Result<JobDto>> Handle(GetJobQuery request, CancellationToken cancellationToken)
    {
        var result = await _cloudConvertClient.GetJob(request.JobId, cancellationToken);

        if (!result.IsSuccess)
        {
            return Result.Error();
        }

        var file = result.Value?.Data.Tasks.Where(x => x.Operation == "export/url").FirstOrDefault()?.Result?.Files?.FirstOrDefault(x => !string.IsNullOrEmpty(x.Url));
        var dto = new JobDto
        {
            Id = result.Value!.Data.Id,
            Status = result.Value.Data.Status,
            Filename = file?.Filename ?? string.Empty,
            Url = file?.Url ?? string.Empty,
        };

        return dto;
    }
}