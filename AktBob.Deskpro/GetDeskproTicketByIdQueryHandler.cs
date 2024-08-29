using AAK.Deskpro;
using AAK.Deskpro.Models;
using AktBob.Deskpro.Contracts;
using Ardalis.Result;
using MediatR;

namespace AktBob.Deskpro;
internal class GetDeskproTicketByIdQueryHandler : IRequestHandler<GetDeskproTicketByIdQuery, Result<Ticket>>
{
    private readonly IDeskproClient _deskproClient;

    public GetDeskproTicketByIdQueryHandler(IDeskproClient deskproClient)
    {
        _deskproClient = deskproClient;
    }

    public async Task<Result<Ticket>> Handle(GetDeskproTicketByIdQuery request, CancellationToken cancellationToken)
    {
        var ticket = await _deskproClient.GetTicketById(request.Id, cancellationToken);

        if (ticket == null)
        {
            return Result.NotFound();
        }

        return ticket;
    }
}
