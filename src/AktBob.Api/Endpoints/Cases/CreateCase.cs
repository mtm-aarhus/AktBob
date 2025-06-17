using AktBob.Shared.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace AktBob.Api.Endpoints.Cases;

internal static class CreateCase
{
    public static void MapCreateCaseEndpoint(this RouteGroupBuilder builder, string route) => builder
        .MapPost(route, EndpointHandler)
        .AllowAnonymous()
        .DisableAntiforgery()
        .WithSummary("Registrér ny case")
        .WithDescription("Registrerer et nyt Podio item i databasen.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);
    private record CreateCaseRequest(long PodioItemId, int DeskproId, string CaseNumber);

    private static async Task<IResult> EndpointHandler(
        [FromServices] CreateCaseTransaction transaction,
        [FromForm] CreateCaseRequest request,
        CancellationToken cancellationToken)
    {
        var result = await transaction.Run(request.PodioItemId, request.DeskproId, request.CaseNumber, cancellationToken);
        return result.ToMinimalApiResponse();
    } 
}