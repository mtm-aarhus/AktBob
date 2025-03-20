namespace AktBob.CloudConvert.Contracts;
internal interface IGetDownloadUrlHandler
{
    Task<Result<string>> Handle(Guid jobId, CancellationToken cancellationToken = default);
}