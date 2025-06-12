using AktBob.Database.Contracts;
using AktBob.Database.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace AktBob.Api.Endpoints.Tickets;

internal static class GetTickets
{
    public static string Description => "Fremsøg tickets i databasen ud fra valgfrie parametre";
    public static string Summery => "Fremsøg tickets";
    internal record GetTicketsRequest(int? DeskproId = null, long? PodioItemId = null, Guid? FilArkivCaseId = null);

    public static async Task<IResult> Endpoint(
        [AsParameters] GetTicketsRequest request,
        [FromServices] ITicketRepository ticketRepository,
        CancellationToken cancellationToken)
    {
        var tickets = await ticketRepository.GetAll(request.DeskproId, request.PodioItemId, request.FilArkivCaseId);
        return Results.Ok(tickets.ToDto());
    } 
}