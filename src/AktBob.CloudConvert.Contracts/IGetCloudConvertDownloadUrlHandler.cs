namespace AktBob.CloudConvert.Contracts;
public interface IGetCloudConvertDownloadUrlHandler
{
    Task<Result<string>> Handle(Guid jobId, CancellationToken cancellationToken = default);
}