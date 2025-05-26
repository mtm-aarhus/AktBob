using AktBob.Database.Contracts;
using AktBob.Database.Dtos;
using AktBob.Database.Extensions;
using AktBob.Shared.Types.Deskpro;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace AktBob.Database.Endpoints.Tickets;

internal record GetTicketsRequest(int? DeskproId, long? PodioItemId, Guid? FilArkivCaseId);

internal class GetTickets(ITicketRepository ticketRepository) : Endpoint<GetTicketsRequest, IEnumerable<TicketDto>>
{
    private readonly ITicketRepository _ticketRepository = ticketRepository;

    public override void Configure()
    {
        Get("/Database/Tickets");
        Options(x => x.WithTags("Database/Tickets"));

        Description(x => x
           .Produces<IEnumerable<TicketDto>>(StatusCodes.Status200OK));
    }

    public override async Task HandleAsync(GetTicketsRequest req, CancellationToken ct)
    {
        var ticketId = req.DeskproId == null
            ? TicketId.Empty
            : TicketId.Create((int)req.DeskproId);

        var tickets = await _ticketRepository.GetAll(ticketId, req.PodioItemId, req.FilArkivCaseId);
        await SendOkAsync(tickets.ToDto());
    }
}
