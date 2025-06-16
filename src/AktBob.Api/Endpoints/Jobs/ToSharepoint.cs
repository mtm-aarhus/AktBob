using AktBob.Shared;
using AktBob.Shared.Contracts.Processors;
using AktBob.Shared.Extensions;
using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Mvc;

namespace AktBob.Api.Endpoints.Jobs;

internal static class ToSharepoint
{
    public static void MapToSharepointEndpoint(this RouteGroupBuilder builder, string route) => builder
        .MapPost(route, Endpoint)
        .AllowAnonymous()
        .DisableAntiforgery()
        .WithSummary("Overfør til Sharepoint")
        .WithDescription("Overfører indhold fra FilArkiv til Sharepoint (via proces i OpenOrchestrator)")
        .Produces(StatusCodes.Status204NoContent);
    
    private record ToSharepointRequest(long PodioItemId);

    private static async Task<IResult> Endpoint(
        [FromServices] IConfiguration configuration,
        [FromServices] IMessageBus messageBus,
        [FromForm] ToSharepointRequest request,
        CancellationToken cancellationToken)
    {
        var queueName = Guard.Against.NullOrEmpty(configuration.GetValue<string>("Queues:ToSharepoint"));
        var job = new ToSharepointJob(request.PodioItemId);
        var result = await messageBus.SendMessage(queueName, job, cancellationToken);
        return result.ToMinimalApiResponse();
    }
}