using AktBob.Shared.CQRS;

namespace AktBob.CloudConvert.UseCases;
internal class GetFileQueryHandler(ICloudConvertClient cloudConvertClient) : IQueryHandler<GetFileQuery, Result<FileDto>>
{
    private readonly ICloudConvertClient _cloudConvertClient = cloudConvertClient;

    public async Task<Result<FileDto>> Handle(GetFileQuery query, CancellationToken cancellationToken)
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
