namespace AktBob.CloudConvert.Contracts;
public interface IGetCloudConvertFileHandler
{
    Task<Result<FileDto>> Handle(string url, CancellationToken cancellationToken = default);
}