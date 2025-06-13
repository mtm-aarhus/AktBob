using AktBob.Database.Contracts;
using AktBob.Database.Extensions;
using AktBob.Shared.Contracts.Database;
using Microsoft.AspNetCore.Mvc;

namespace AktBob.Api.Endpoints.Cases;

public static class GetCase
{
    public static void MapGetCaseEndpoint(this RouteGroupBuilder builder, string route) => builder
        .MapGet(route, EndpointHandler)
        .WithSummary("Hent case")
        .WithDescription("Henter case fra databasen.")
        .Produces<CaseDto>()
        .ProducesProblem(StatusCodes.Status404NotFound);
    
    private static async Task<IResult> EndpointHandler(
        [FromRoute] int id,
        [FromServices] ICaseRepository repository)
    {
        var @case = await repository.Get(id);
        return @case == null ? Results.NotFound() : Results.Ok(@case.ToDto());
    }
}