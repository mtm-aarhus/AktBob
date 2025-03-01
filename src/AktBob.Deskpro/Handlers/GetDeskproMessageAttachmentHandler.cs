using AAK.Deskpro;

namespace AktBob.Deskpro.Handlers;
public class GetDeskproMessageAttachmentHandler(IDeskproClient deskproClient) : IGetDeskproMessageAttachmentHandler
{
    private readonly IDeskproClient _deskproClient = deskproClient;

    public async Task<Result<Stream>> Handle(string downloadUrl, CancellationToken cancellationToken)
    {
        var stream = await _deskproClient.DownloadAttachment(downloadUrl, cancellationToken);

        if (stream == null)
        {
            return Result.NotFound();
        }

        return Result.Success(stream);
    }
}