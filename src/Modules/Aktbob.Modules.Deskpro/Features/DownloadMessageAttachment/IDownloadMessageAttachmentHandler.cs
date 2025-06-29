namespace Aktbob.Modules.Deskpro.Features.DownloadMessageAttachment;
public interface IDownloadMessageAttachmentHandler
{
    Task<ErrorOr<Stream>> Handle(string downloadUrl, CancellationToken cancellationToken);
}