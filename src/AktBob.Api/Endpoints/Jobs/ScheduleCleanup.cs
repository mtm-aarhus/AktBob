using AktBob.Database.Contracts;
using AktBob.Shared;
using AktBob.Shared.Contracts.Processors;
using AktBob.Shared.Exceptions;
using AktBob.Shared.Extensions;
using Microsoft.AspNetCore.Mvc;
using Ardalis.GuardClauses;

namespace AktBob.Api.Endpoints.Jobs;

internal static class ScheduleCleanup
{
    public static void MapScheduleCleanupEndpoint(this RouteGroupBuilder builder, string route) => builder
        .MapPost(route, EndpointHandler)
        .WithSummary("Opret oprydningsjobs")
        .WithDescription("Opretter oprydningsjob for hhv. FilArkiv og Sharepoint for det angivne Deskpro item")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);
    
    private record ScheduleCleanupRequest(int DeskproTicketId);

    private static async Task<IResult> EndpointHandler(
        [FromBody] ScheduleCleanupRequest request,
        [FromServices] IMessageBus messageBus,
        [FromServices] ITicketRepository ticketRepository,
        [FromServices] IConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var ticket = await ticketRepository.GetByDeskproTicketId(request.DeskproTicketId);
        if (ticket is null) return Results.NotFound($"Deskpro ticket {request.DeskproTicketId} not found in database");

        if (ticket.CleanUpScheduledAt.HasValue) return Results.NoContent();
        
        var cleanupQueueName = Guard.Against.NullOrEmpty(configuration.GetValue<string>("Queues:Cleanup"));
        var cleanupNotifyQueueName = Guard.Against.NullOrEmpty(configuration.GetValue<string>("Queues:CleanupNotify"));

        var cleanUpMessageSent = await messageBus.SendMessage(cleanupQueueName, new CleanupJob(request.DeskproTicketId), cancellationToken);
        var cleanUpNotifyMessageSent = await messageBus.SendMessage(cleanupNotifyQueueName, new CleanupNotifyJob(request.DeskproTicketId), cancellationToken);
        
        if (cleanUpMessageSent.IsError) return Results.InternalServerError(error: "Error dispatch clean up message");
        if (cleanUpNotifyMessageSent.IsError) return Results.InternalServerError(error: "Error dispatch clean up notification message");
        
        ticket.CleanUpScheduledAt = DateTime.UtcNow;
        var updateTicketSuccess = await ticketRepository.Update(ticket);
        return !updateTicketSuccess ? Results.InternalServerError("Error updating database ticket") : Results.NoContent();
    }
}