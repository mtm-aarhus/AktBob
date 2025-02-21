using AAK.Deskpro;
using AktBob.Deskpro.Contracts;
using Ardalis.Result;
using MassTransit.Mediator;

namespace AktBob.Deskpro.UseCases;
public class GetDeskproMessageAttachmentQueryHandler(IDeskproClient deskproClient) : MediatorRequestHandler<GetDeskproMessageAttachmentQuery, Result<Stream>>
{
    private readonly IDeskproClient _deskproClient = deskproClient;

    protected override async Task<Result<Stream>> Handle(GetDeskproMessageAttachmentQuery query, CancellationToken cancellationToken)
    {
        var stream = await _deskproClient.DownloadAttachment(query.DownloadUrl, cancellationToken);

        if (stream == null)
        {
            return Result.NotFound();
        }

        return stream;
    }
}
