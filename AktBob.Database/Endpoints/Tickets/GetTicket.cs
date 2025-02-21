using AktBob.Database.Contracts.Dtos;
using AktBob.Database.Extensions;
using AktBob.Database.UseCases.Tickets;
using FastEndpoints;
using MassTransit;
using MassTransit.Mediator;
using Microsoft.AspNetCore.Http;

namespace AktBob.Database.Endpoints.Tickets;

internal record GetTicketRequest(int Id);

internal class GetTicket(IMediator mediator) : Endpoint<GetTicketRequest, TicketDto>
{
    private readonly IMediator _mediator = mediator;

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
        var query = new GetTicketByIdQuery(req.Id);
        var result = await _mediator.SendRequest(query, ct);

        if (!result.IsSuccess)
        {
            await SendNotFoundAsync();
            return;
        }

        var dto = result.Value;
        await SendOkAsync(dto);
    }
}
