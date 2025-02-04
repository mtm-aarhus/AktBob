using AktBob.CloudConvert.Contracts;
using AktBob.CloudConvert.Contracts.DTOs;
using Ardalis.Result;
using MassTransit.Mediator;

namespace AktBob.CloudConvert;
public class GetFileQueryHandler(ICloudConvertClient cloudConvertClient) : MediatorRequestHandler<GetFileQuery, Result<FileDto>>
{
    private readonly ICloudConvertClient _cloudConvertClient = cloudConvertClient;

    protected override async Task<Result<FileDto>> Handle(GetFileQuery query, CancellationToken cancellationToken)
    {
        var result = await _cloudConvertClient.GetFile(query.Url, cancellationToken);

        if (!result.IsSuccess)
        {
            return Result.Error();
        }

        var dto = new FileDto(result.Value.Stream, result.Value.Filename);
        return Result.Success(dto);
    }
}
