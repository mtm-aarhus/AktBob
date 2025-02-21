using AktBob.Database.Contracts;
using AktBob.Database.Contracts.Dtos;
using AktBob.Database.Extensions;
using FastEndpoints;
using MassTransit;
using MassTransit.Mediator;
using Microsoft.AspNetCore.Http;

namespace AktBob.Database.Endpoints.Tickets;

internal record GetTicketsRequest(int? DeskproId, long? PodioItemId, Guid? FilArkivCaseId, bool IncludeClosedTickets = true);

internal class GetTickets(IMediator mediator) : Endpoint<GetTicketsRequest, IEnumerable<TicketDto>>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("/Database/Tickets");
        Options(x => x.WithTags("Database/Tickets"));

        Description(x => x
           .Produces<IEnumerable<TicketDto>>(StatusCodes.Status200OK));
    }

    public override async Task HandleAsync(GetTicketsRequest req, CancellationToken ct)
    {
        var query = new GetTicketsQuery(
            DeskproId: req.DeskproId,
            PodioItemId: req.PodioItemId,
            FilArkivCaseId: req.FilArkivCaseId,
            IncludeClosedTickets: req.IncludeClosedTickets);

        var result = await _mediator.SendRequest(query, ct);

        await this.SendResponse(result, r => r.Value);
    }
}
