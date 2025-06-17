using AktBob.Shared;
using AktBob.Shared.Contracts.Processors;
using AktBob.Shared.Extensions;
using Microsoft.AspNetCore.Mvc;
using Ardalis.GuardClauses;

namespace AktBob.Api.Endpoints.Submissions;

internal static class CreateSubmission
{
    public static void MapCreateSubmissionEndpoint(this RouteGroupBuilder builder, string route) => builder
        .MapPost(route, EndpointHandler)
        .WithSummary("Tilføj submission")
        .WithDescription("Tilføjer submission fra OS2Forms i databasen.")
        .Produces(StatusCodes.Status204NoContent);
    private record CreateSubmissionRequest(Guid SubmissionId, int TicketId);

    private static async Task<IResult> EndpointHandler(
        [FromBody] CreateSubmissionRequest request,
        [FromServices] IMessageBus messageBus,
        [FromServices] IConfiguration configuration,
        CancellationToken cancellation)
    {
        var queueName = Guard.Against.NullOrEmpty(configuration.GetValue<string>("Queues:RegisterOS2FormsSubmission"));
        var job = new RegisterOS2FormsSubmissionJob(request.SubmissionId, request.TicketId);
        var result = await messageBus.SendMessage(queueName, job, cancellation);
        return result.ToMinimalApiResponse();
    }
}