namespace AktBob.CloudConvert.Contracts;
internal interface IGetDownloadUrlHandler
{
    Task<ErrorOr<string>> Handle(Guid jobId, CancellationToken cancellationToken = default);
}