namespace AktBob.CloudConvert.Handlers.DownloadFile;

internal class DownloadFileHandler(ICloudConvertClient cloudConvertClient) : IDownloadFileHandler
{
    private readonly ICloudConvertClient _cloudConvertClient = cloudConvertClient;

    public async Task<ErrorOr<byte[]>> Handle(string url, CancellationToken cancellationToken = default) => await _cloudConvertClient.GetFile(url, cancellationToken);
}