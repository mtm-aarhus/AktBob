using AAK.Deskpro;
using AAK.Deskpro.Models;
using AktBob.Deskpro.Contracts;
using Ardalis.Result;
using MediatR;

namespace AktBob.Deskpro;
internal class GetDeskproMessageByIdQueryHandler : IRequestHandler<GetDeskproMessageByIdQuery, Result<Message>>
{
    private readonly IDeskproClient _deskproClient;

    public GetDeskproMessageByIdQueryHandler(IDeskproClient deskproClient)
    {
        _deskproClient = deskproClient;
    }

    public async Task<Result<Message>> Handle(GetDeskproMessageByIdQuery request, CancellationToken cancellationToken)
    {
        var message = await _deskproClient.GetMessage(request.TicketId, request.MessageId, cancellationToken);

        if (message == null)
        {
            return Result.NotFound();
        }

        return Result.Success(message);
    }
}
