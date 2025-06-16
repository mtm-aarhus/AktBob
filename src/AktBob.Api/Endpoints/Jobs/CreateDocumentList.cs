using AktBob.Shared;
using AktBob.Shared.Contracts.Processors;
using AktBob.Shared.Extensions;
using Microsoft.AspNetCore.Mvc;
using Ardalis.GuardClauses;

namespace AktBob.Api.Endpoints.Jobs;

internal static class CreateDocumentList
{
    public static void MapCreateDocumentListEndpoint(this RouteGroupBuilder builder, string route) => builder
        .MapPost(route, Endpoint)
        .AllowAnonymous()
        .DisableAntiforgery()
        .WithSummary("Opret dokumentliste")
        .WithDescription("Opretter dokumentliste for sagen på det angivne Podio item (via proces i OpenOrchestrator")
        .Produces(StatusCodes.Status204NoContent);
    
    private record CreateDocumentListRequest(long PodioItemId);

    private static async Task<IResult> Endpoint(
        [FromServices] IConfiguration configuration,
        [FromServices] IMessageBus messageBus,
        [FromForm] CreateDocumentListRequest request,
        CancellationToken cancellationToken)
    {
        var queueName = Guard.Against.NullOrEmpty(configuration.GetValue<string>("Queues:CreateDocumentList"));
        var job = new CreateDocumentListJob(request.PodioItemId);
        var result = await messageBus.SendMessage(queueName, job, cancellationToken);
        return result.ToMinimalApiResponse();
    }
}