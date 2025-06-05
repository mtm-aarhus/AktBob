using AAK.Deskpro;

namespace Aktbob.Modules.Deskpro.Features.DownloadMessageAttachment;
public class DownloadMessageAttachmentHandler(IDeskproClient deskproClient) : IDownloadMessageAttachmentHandler
{
    private readonly IDeskproClient _deskproClient = deskproClient;

    public async Task<ErrorOr<Stream>> Handle(string downloadUrl, CancellationToken cancellationToken) => await _deskproClient.DownloadAttachment(downloadUrl, cancellationToken);
}