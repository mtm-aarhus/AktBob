using AktBob.Database.Contracts.Dtos;
using AktBob.Database.Extensions;
using AktBob.Database.UseCases.Tickets.GetTicketById;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace AktBob.Database.Endpoints.Tickets.Get;
internal class GetTicket : Endpoint<GetTicketRequest, TicketDto>
{
    private readonly IMediator _mediator;

    public GetTicket(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/Database/Tickets/{Id}");
        Options(x => x.WithTags("Database/Tickets"));

        Description(x => x
           .Produces<TicketDto>(StatusCodes.Status200OK)
           .ProducesProblem(StatusCodes.Status404NotFound));

        Summary(x =>
        {
            x.Description = "Henter en specific ticket i databasen ud fra database-ID'et";
        });
    }


    public override async Task HandleAsync(GetTicketRequest req, CancellationToken ct)
    {
        var query = new GetTicketByIdQuery(req.Id);
        var result = await _mediator.Send(query, ct);

        if (!result.IsSuccess)
        {
            await SendNotFoundAsync();
            return;
        }

        var dto = result.Value.ToDto();
        await SendOkAsync(dto);
    }
}
