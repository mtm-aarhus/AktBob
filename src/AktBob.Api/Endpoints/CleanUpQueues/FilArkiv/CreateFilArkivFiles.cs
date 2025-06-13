using AktBob.Database.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace AktBob.Api.Endpoints.CleanUpQueues.FilArkiv;

internal static class CreateFilArkivFiles
{
    public static void MapCreateFilArkivFilesEndpoint(this RouteGroupBuilder builder, string route) => builder
        .MapPost(route, EndpointHandler)
        .WithSummary("Tilføj FilArkiv filer til oprydningskø")
        .WithDescription("Tilføj liste af FilArkiv fil ID'er til oprydningskø")
        .Produces(StatusCodes.Status204NoContent);
        
    private record CreateItemsRequest(Guid[] Files);
    
    private static async Task<IResult> EndpointHandler(
        [FromBody] CreateItemsRequest request,
        [FromServices] IFilArkivFilesCleanUpQueueRepository repository)
    {
        foreach (var file in request.Files)
        {
            await repository.Add(file);
        }
        
        return Results.NoContent();
    }
}