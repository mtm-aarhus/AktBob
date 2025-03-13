namespace AktBob.CloudConvert.Contracts;
internal interface IGettDownloadUrlHandler
{
    Task<Result<string>> Handle(Guid jobId, CancellationToken cancellationToken = default);
}