namespace AktBob.CloudConvert.Handlers.GetDownloadUrl;

internal interface IGetDownloadUrlHandler
{
    Task<ErrorOr<string>> Handle(Guid jobId, CancellationToken cancellationToken = default);
}
