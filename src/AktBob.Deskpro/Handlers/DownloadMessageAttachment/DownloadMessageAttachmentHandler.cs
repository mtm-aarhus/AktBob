using AAK.Deskpro;

namespace AktBob.Deskpro.Handlers.DownloadMessageAttachment;
public class DownloadMessageAttachmentHandler(IDeskproClient deskproClient) : IDownloadMessageAttachmentHandler
{
    private readonly IDeskproClient _deskproClient = deskproClient;

    public async Task<ErrorOr<Stream>> Handle(string downloadUrl, CancellationToken cancellationToken)
    {
        var stream = await _deskproClient.DownloadAttachment(downloadUrl, cancellationToken);
        if (stream == null)
        {
            return Error.Failure("DownloadMessageAttachmentHandler.StreamIsNull", $"Stream is null. Download url: {downloadUrl}");
        }

        return stream;
    }
}