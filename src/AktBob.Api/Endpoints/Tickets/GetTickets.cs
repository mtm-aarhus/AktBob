using AktBob.Database.Contracts;
using AktBob.Database.Extensions;
using AktBob.Shared.Contracts.Database;
using Microsoft.AspNetCore.Mvc;

namespace AktBob.Api.Endpoints.Tickets;

internal static class GetTickets
{
    public static void MapGetTicketsEndpoint(this RouteGroupBuilder builder, string routePattern) => builder
        .MapGet(routePattern, EndpointHandler)
        .WithSummary("Fremsøg tickets")
        .WithDescription("Fremsøg tickets i databasen ud fra valgfrie parametre.")
        .Produces<TicketDto[]>();
    
    private record GetTicketsRequest(int? DeskproId = null, long? PodioItemId = null, Guid? FilArkivCaseId = null);

    private static async Task<IResult> EndpointHandler(
        [AsParameters] GetTicketsRequest request,
        [FromServices] ITicketRepository ticketRepository,
        CancellationToken cancellationToken)
    {
        var tickets = await ticketRepository.GetAll(request.DeskproId, request.PodioItemId, request.FilArkivCaseId);
        return Results.Ok(tickets.ToDto());
    } 
}