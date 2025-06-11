using AktBob.Database.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace AktBob.Api.Endpoints.Tickets;

internal record UpdateTicketRequest(string? CaseNumber = null, string? CaseUrl = null, string? SharepointFolderName = null);

internal static class UpdateTicket
{
    public static string Description => "Opdaterer ticket i databasen. Alle properties i body'en er valgfrie.";
    public static string Summery => "Opdatér ticket";
    
    public static async Task<IResult> Endpoint(
        [FromRoute] int id,
        [FromBody]  UpdateTicketRequest request,
        [FromServices] ITicketRepository ticketRepository,
        CancellationToken cancellationToken)
    {
        // Get existing entity from repository
        var ticket = await ticketRepository.Get(id);
        if (ticket == null) return Results.NotFound();

        // Update entity properties
        if (!string.IsNullOrEmpty(request.CaseNumber))
        {
            ticket.CaseNumber = request.CaseNumber;
        }

        if (!string.IsNullOrEmpty(request.CaseUrl))
        {
            ticket.CaseUrl = request.CaseUrl;
        }

        if (!string.IsNullOrEmpty(request.SharepointFolderName))
        {
            ticket.SharepointFolderName = request.SharepointFolderName;
        }

        // Update
        var updated = await ticketRepository.Update(ticket);

        // Response
        if (updated)
        {
            return Results.NoContent();
        }
        
        return Results.Problem(new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Detail = $"Something went wrong updating ticket {id}"
        });
    }
}