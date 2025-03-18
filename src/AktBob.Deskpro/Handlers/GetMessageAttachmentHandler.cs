using AAK.Deskpro;

namespace AktBob.Deskpro.Handlers;
public class GetMessageAttachmentHandler(IDeskproClient deskproClient) : IGetMessageAttachmentHandler
{
    private readonly IDeskproClient _deskproClient = deskproClient;

    public async Task<Result<Stream>> Handle(string downloadUrl, CancellationToken cancellationToken)
    {
        try
        {
            var stream = await _deskproClient.DownloadAttachment(downloadUrl, cancellationToken);
            if (stream == null)
            {
                return Result.Error($"Error downloading attachment from Deskpro (download URL: {downloadUrl})");
            }

            return Result.Success(stream);
        }
        catch (HttpRequestException ex)
        {
            return Result.Error($"Error download attachment from Deskpro ({downloadUrl}): {ex}");
        }
        catch (Exception)
        {
            throw;
        }
    }
}