using AktBob.Database.Contracts;
using AktBob.Database.Extensions;
using AktBob.Shared.Contracts.Database;
using Microsoft.AspNetCore.Mvc;

namespace AktBob.Api.Endpoints.Tickets;

internal static class GetTicket
{
    public static void MapGetTicketEndpoint(this RouteGroupBuilder builder, string routePattern) => builder
        .MapGet(routePattern, EndpointHandler)
        .WithSummary("Hent ticket")
        .WithDescription("Henter ticket fra databasen.")
        .Produces<TicketDto>();

    private static async Task<IResult> EndpointHandler(
        [FromRoute] int id,
        [FromServices] ITicketRepository repository,
        CancellationToken cancellationToken)
    {
        var ticket = await repository.Get(id);
        return ticket is null ? Results.NotFound() : Results.Ok(ticket.ToDto());
    }
}