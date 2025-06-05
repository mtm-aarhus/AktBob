namespace Aktbob.Modules.Deskpro.Features.DownloadMessageAttachment;
internal interface IDownloadMessageAttachmentHandler
{
    Task<ErrorOr<Stream>> Handle(string downloadUrl, CancellationToken cancellationToken);
}