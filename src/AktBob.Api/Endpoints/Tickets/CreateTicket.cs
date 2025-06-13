using AktBob.Database.Contracts;
using AktBob.Database.Entities;
using Microsoft.AspNetCore.Mvc;

namespace AktBob.Api.Endpoints.Tickets;

internal static class CreateTicket
{
    private record CreateTicketRequest(int DeskproId);
    
    public static void MapCreateTicketEndpoint(this RouteGroupBuilder builder, string routePattern) => builder
        .MapPost(routePattern, Endpoint)
        .WithSummary("Registrer ny ticket")
        .WithDescription("Registrerer en ny ticket i databasen baseret på et Deskpro ticket ID.")
        .Produces(StatusCodes.Status204NoContent);
    
    private static async Task<IResult> Endpoint(
        [FromBody] CreateTicketRequest request,
        [FromServices] ITicketRepository repository,
        CancellationToken cancellationToken)
    {
        var ticket = new Ticket
        {
            DeskproId = request.DeskproId
        };
        
        var success = await repository.Add(ticket);
        if (success)
        {
            return Results.NoContent();
        }

        return Results.Problem(new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Detail = $"Something went wrong creating new ticket in the database with DeskproId = {request.DeskproId}"
        });
    }
}