namespace AktBob.CloudConvert.Contracts;
public interface IGetCloudConvertFileHandler
{
    Task<Result<byte[]>> Handle(string url, CancellationToken cancellationToken = default);
}