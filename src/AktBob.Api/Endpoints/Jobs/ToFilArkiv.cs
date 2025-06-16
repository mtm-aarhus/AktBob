using AktBob.Shared;
using AktBob.Shared.Contracts.Processors;
using AktBob.Shared.Extensions;
using Microsoft.AspNetCore.Mvc;
using Ardalis.GuardClauses;

namespace AktBob.Api.Endpoints.Jobs;

internal static class ToFilArkiv
{
    public static void MapToFilArkivEndpoint(this RouteGroupBuilder builder, string route) => builder
        .MapPost(route, Endpoint)
        .DisableAntiforgery()
        .WithSummary("Overfør til FilArkiv")
        .WithDescription("Overfører indhold fra til FilArkiv (via proces i OpenOrchestrator)")
        .Produces(StatusCodes.Status204NoContent);

    private record ToFilArkivQueueItemRequest(long PodioItemId);

    private static async Task<IResult> Endpoint(
        [FromServices] IConfiguration configuration,
        [FromServices] IMessageBus messageBus,
        [FromForm] ToFilArkivQueueItemRequest request,
        CancellationToken cancellationToken)
    {
        var queueName = Guard.Against.NullOrEmpty(configuration.GetValue<string>("Queues:ToFilArkiv"));
        var job = new ToFilArkivJob(request.PodioItemId);
        var result = await messageBus.SendMessage(queueName, job, cancellationToken);
        return result.ToMinimalApiResponse();
    }
}