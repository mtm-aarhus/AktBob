using AktBob.Database.Contracts;
using AktBob.Database.Dtos;
using AktBob.Database.Extensions;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace AktBob.Database.Endpoints.Tickets;

internal record PatchTicketRequest
{
    public int Id { get; set; }
    public string? CaseNumber { get; set; }
    public string? CaseUrl { get; set; }
    public string? SharepointFolderName { get; set; }
}

internal class PatchTicket(ITicketRepository ticketRepository) : Endpoint<PatchTicketRequest, TicketDto>
{
    private readonly ITicketRepository _ticketRepository = ticketRepository;

    public override void Configure()
    {
        Patch("/Database/Tickets/{Id}");
        Options(x => x.WithTags("Database/Tickets"));

        Description(x => x
           .Produces<TicketDto>(StatusCodes.Status200OK)
           .ProducesProblem(StatusCodes.Status404NotFound));
    }

    public override async Task HandleAsync(PatchTicketRequest req, CancellationToken ct)
    {
        // Get existing entity from repository
        var ticket = await _ticketRepository.Get(req.Id);

        if (ticket == null)
        {
            await SendNotFoundAsync();
            return;
        }


        // Update entity properties
        if (!string.IsNullOrEmpty(req.CaseNumber))
        {
            ticket.CaseNumber = req.CaseNumber;
        }

        if (!string.IsNullOrEmpty(req.CaseUrl))
        {
            ticket.CaseUrl = req.CaseUrl;
        }

        if (!string.IsNullOrEmpty(req.SharepointFolderName))
        {
            ticket.SharepointFolderName = req.SharepointFolderName;
        }


        // Update
        var updated = await _ticketRepository.Update(ticket) == 1;


        // Response
        if (updated)
        {
            var updatedTicket = await _ticketRepository.Get(req.Id);

            if (updatedTicket is null)
            {
                await SendErrorsAsync();
                return;
            }

            await SendOkAsync(updatedTicket.ToDto());
            return;
        }

        await SendErrorsAsync();
    }
}
