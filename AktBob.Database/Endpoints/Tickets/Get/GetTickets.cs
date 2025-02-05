using AktBob.Database.Contracts.Dtos;
using AktBob.Database.Extensions;
using AktBob.Database.UseCases.Tickets.GetTickets;
using FastEndpoints;
using MassTransit;
using MassTransit.Mediator;
using Microsoft.AspNetCore.Http;

namespace AktBob.Database.Endpoints.Tickets.Get;
internal class GetTickets(IMediator mediator) : Endpoint<GetTicketsRequest, IEnumerable<TicketDto>>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("/Database/Tickets");
        Options(x => x.WithTags("Database/Tickets"));

        Description(x => x
           .Produces<IEnumerable<TicketDto>>(StatusCodes.Status200OK));

        Summary(x =>
        {
            x.Description = "Henter alle tickets fra databasen der matcher den eller de angivne filtreringsfelter. Hvis ingen filtreringsfelter angives returnerers samtlige tickets fra databasen.";
        });
    }

    public override async Task HandleAsync(GetTicketsRequest req, CancellationToken ct)
    {
        var query = new GetTicketsQuery(
            DeskproId: req.DeskproId,
            PodioItemId: req.PodioItemId,
            FilArkivCaseId: req.FilArkivCaseId,
            IncludeClosedTickets: req.IncludeClosedTickets);

        var result = await _mediator.SendRequest(query, ct);

        await this.SendResponse(result, r => r.Value.ToDto());
    }
}
