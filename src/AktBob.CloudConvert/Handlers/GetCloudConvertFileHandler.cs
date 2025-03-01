namespace AktBob.CloudConvert.Handlers;
internal class GetCloudConvertFileHandler(ICloudConvertClient cloudConvertClient) : IGetCloudConvertFileHandler
{
    private readonly ICloudConvertClient _cloudConvertClient = cloudConvertClient;

    public async Task<Result<FileDto>> Handle(string url, CancellationToken cancellationToken = default)
    {
        var result = await _cloudConvertClient.GetFile(url, cancellationToken);

        if (!result.IsSuccess)
        {
            return Result.Error();
        }

        var dto = new FileDto(result.Value.Stream, result.Value.Filename);
        return Result.Success(dto);
    }
}