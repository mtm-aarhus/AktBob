namespace AktBob.Deskpro.Handlers.DownloadMessageAttachment;
internal interface IDownloadMessageAttachmentHandler
{
    Task<ErrorOr<Stream>> Handle(string downloadUrl, CancellationToken cancellationToken);
}