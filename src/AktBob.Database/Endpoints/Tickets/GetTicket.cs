using AktBob.Database.Contracts;
using AktBob.Database.Dtos;
using AktBob.Database.Extensions;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace AktBob.Database.Endpoints.Tickets;

internal record GetTicketRequest(int Id);

internal class GetTicket(ITicketRepository ticketRepository) : Endpoint<GetTicketRequest, TicketDto>
{
    private readonly ITicketRepository _ticketRepository = ticketRepository;

    public override void Configure()
    {
        Get("/Database/Tickets/{Id}");
        Options(x => x.WithTags("Database/Tickets"));

        Description(x => x
           .Produces<TicketDto>(StatusCodes.Status200OK)
           .ProducesProblem(StatusCodes.Status404NotFound));
    }


    public override async Task HandleAsync(GetTicketRequest req, CancellationToken ct)
    {
        var ticket = await _ticketRepository.Get(req.Id);

        if (ticket is null)
        {
            await SendNotFoundAsync();
            return;
        }

        await SendOkAsync(ticket.ToDto());
    }
}