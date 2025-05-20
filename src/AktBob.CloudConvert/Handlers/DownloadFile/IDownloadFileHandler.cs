namespace AktBob.CloudConvert.Handlers.DownloadFile;

internal interface IDownloadFileHandler
{
    Task<ErrorOr<byte[]>> Handle(string url, CancellationToken cancellationToken = default);
}
