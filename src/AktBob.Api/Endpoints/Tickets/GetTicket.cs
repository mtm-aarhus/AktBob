using AktBob.Database.Contracts;
using AktBob.Database.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace AktBob.Api.Endpoints.Tickets;

internal static class GetTicket
{
    public static string Description => "Henter ticket fra databasen";
    public static string Summery => "Hent ticket";
    
    public static async Task<IResult> Endpoint(
        [FromRoute] int id,
        [FromServices] ITicketRepository repository,
        CancellationToken cancellationToken)
    {
        var ticket = await repository.Get(id);
        return ticket is null ? Results.NotFound() : Results.Ok(ticket.ToDto());
    }
}