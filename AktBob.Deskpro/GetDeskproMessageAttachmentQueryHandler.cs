using AAK.Deskpro;
using AktBob.Deskpro.Contracts;
using Ardalis.Result;
using MediatR;

namespace AktBob.Deskpro;
internal class GetDeskproMessageAttachmentQueryHandler(IDeskproClient deskproClient) : IRequestHandler<GetDeskproMessageAttachmentQuery, Result<Stream>>
{
    private readonly IDeskproClient _deskproClient = deskproClient;

    public async Task<Result<Stream>> Handle(GetDeskproMessageAttachmentQuery query, CancellationToken cancellationToken)
    {
        var stream = await _deskproClient.DownloadAttachment(query.DownloadUrl, cancellationToken);

        if (stream == null)
        {
            return Result.NotFound();
        }

        return stream;
    }
}
