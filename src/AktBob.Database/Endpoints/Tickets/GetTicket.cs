using AktBob.Database.Contracts.Dtos;
using AktBob.Database.UseCases.Tickets;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace AktBob.Database.Endpoints.Tickets;

internal record GetTicketRequest(int Id);

internal class GetTicket(IQueryDispatcher queryDispatcher) : Endpoint<GetTicketRequest, TicketDto>
{
    private readonly IQueryDispatcher _queryDispatcher = queryDispatcher;

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
        var result = await _queryDispatcher.Dispatch(query, ct);

        if (!result.IsSuccess)
        {
            await SendNotFoundAsync();
            return;
        }

        var dto = result.Value;
        await SendOkAsync(dto);
    }
}
