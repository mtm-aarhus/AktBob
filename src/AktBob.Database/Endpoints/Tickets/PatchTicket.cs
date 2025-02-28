using AktBob.Database.Contracts.Dtos;
using AktBob.Database.Extensions;
using AktBob.Database.UseCases.Tickets.UpdateTicket;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace AktBob.Database.Endpoints.Tickets;

internal record PatchTicketRequest
{
    public int Id { get; set; }
    public string? CaseNumber { get; set; }
    public string? CaseUrl { get; set; }
    public string? SharepointFolderName { get; set; }
    public DateTime? TicketClosedAt { get; set; }
    public DateTime? JournalizedAt { get; set; }
}

internal class PatchTicket(IMediator mediator) : Endpoint<PatchTicketRequest, TicketDto>
{
    private readonly IMediator _mediator = mediator;

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
        var command = new UpdateTicketCommand(
            Id: req.Id,
            CaseNumber: req.CaseNumber,
            CaseUrl: req.CaseUrl,
            SharepointFolderName: req.SharepointFolderName,
            TicketClosedAt: req.TicketClosedAt,
            JournalizedAt: req.JournalizedAt);

        var result = await _mediator.Send(command, ct);
        await this.SendResponse(result, r => r.Value);
    }
}
