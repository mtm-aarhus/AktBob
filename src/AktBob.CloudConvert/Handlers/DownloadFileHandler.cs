namespace AktBob.CloudConvert.Handlers;
internal class DownloadFileHandler(ICloudConvertClient cloudConvertClient) : IDownloadFileHandler
{
    private readonly ICloudConvertClient _cloudConvertClient = cloudConvertClient;

    public async Task<Result<byte[]>> Handle(string url, CancellationToken cancellationToken = default) => await _cloudConvertClient.GetFile(url, cancellationToken);
}