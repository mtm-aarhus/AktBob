namespace AktBob.CloudConvert.Contracts;
internal interface IDownloadFileHandler
{
    Task<ErrorOr<byte[]>> Handle(string url, CancellationToken cancellationToken = default);
}