using AktBob.Database.Contracts.Dtos;
using AktBob.Database.Endpoints.Tickets;
using AktBob.Database.Endpoints.Tickets.Get;
using AktBob.Database.Extensions;
using AktBob.Database.UseCases.Tickets.AddTicket;
using FastEndpoints;
using MassTransit;
using MassTransit.Mediator;
using Microsoft.AspNetCore.Http;

namespace AktBob.Database.Entities.Tickets;
internal class PostTicket(IMediator mediator) : Endpoint<PostTicketRequest, TicketDto>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Post("/Database/Tickets");
        Options(x => x.WithTags("Database/Tickets"));

        Description(x => x
          .Produces<TicketDto>(StatusCodes.Status201Created)
          .ProducesProblem(StatusCodes.Status400BadRequest));

        Summary(x =>
        {
            x.Description = "Opretter en ny ticket i databasen.";
        });
    }

    public override async Task HandleAsync(PostTicketRequest req, CancellationToken ct)
    {
        var command = new AddTicketCommand(req.DeskproId);
        var result = await _mediator.SendRequest(command);

        if (result.IsSuccess)
        {
            await SendCreatedAtAsync<GetTicket>(routeValues: null, responseBody: result.Value.ToDto(), cancellation: ct);
            return;
        }

        await this.SendResponse(result, r => r.Value.ToDto());
    }
}
