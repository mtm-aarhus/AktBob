using AktBob.Database.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace AktBob.Api.Endpoints.Cases;

internal static class UpdateCase
{
    public static void MapUpdateCaseEndpoint(this RouteGroupBuilder builder, string route) => builder
        .MapPatch(route, EndpointHandler)
        .WithSummary("Opdater case")
        .WithDescription("Opdaterer den angivne case i databasen. Alle parametre i body'en er valgfrie.")
        .Produces(StatusCodes.Status204NoContent);
    
    private record UpdateCaseRequest(
        long? PodioItemId = null,
        string? CaseNumber = null,
        Guid? FilArkivCaseId = null,
        string? SharepointFolderName = null);

    private static async Task<IResult> EndpointHandler(
        [FromRoute] int id,
        [FromBody] UpdateCaseRequest request,
        [FromServices] ICaseRepository repository)
    {
        // Get existing case from repository
        var @case = await repository.Get(id);
        if (@case == null) return Results.NotFound();

        // Update case properties
        if (!string.IsNullOrEmpty(request.CaseNumber))
        {
            @case.CaseNumber = request.CaseNumber;
        }

        if (!string.IsNullOrEmpty(request.SharepointFolderName))
        {
            @case.SharepointFolderName = request.SharepointFolderName;
        }

        @case.PodioItemId = request.PodioItemId ?? @case.PodioItemId;
        @case.FilArkivCaseId = request.FilArkivCaseId ?? @case.FilArkivCaseId;

        // Update entity
        var updated = await repository.Update(@case);

        // Response
        if (updated)
        {
            return Results.NoContent();
        }

        return Results.Problem(new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Detail = $"Something went wrong updating case {id}"
        });
    }
}