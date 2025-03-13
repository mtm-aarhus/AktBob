namespace AktBob.CloudConvert.Contracts;
internal interface IDownloadFileHandler
{
    Task<Result<byte[]>> Handle(string url, CancellationToken cancellationToken = default);
}