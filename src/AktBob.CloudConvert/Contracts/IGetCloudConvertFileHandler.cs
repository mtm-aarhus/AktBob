namespace AktBob.CloudConvert.Contracts;
internal interface IGetCloudConvertFileHandler
{
    Task<Result<byte[]>> Handle(string url, CancellationToken cancellationToken = default);
}