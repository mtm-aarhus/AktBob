namespace AktBob.CloudConvert.Contracts;
internal interface IGetCloudConvertDownloadUrlHandler
{
    Task<Result<string>> Handle(Guid jobId, CancellationToken cancellationToken = default);
}