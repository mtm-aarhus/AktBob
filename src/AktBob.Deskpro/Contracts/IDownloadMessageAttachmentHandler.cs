namespace AktBob.Deskpro.Contracts;
internal interface IDownloadMessageAttachmentHandler
{
    Task<Result<Stream>> Handle(string downloadUrl, CancellationToken cancellationToken);
}