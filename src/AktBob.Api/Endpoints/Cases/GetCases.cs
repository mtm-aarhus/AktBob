using AktBob.Database.Contracts;
using AktBob.Database.Extensions;
using AktBob.Shared.Contracts.Database;
using Microsoft.AspNetCore.Mvc;

namespace AktBob.Api.Endpoints.Cases;

internal static class GetCases
{
    public static void MapGetCasesEndpoint(this RouteGroupBuilder builder, string route) => builder
        .MapGet(route, EndpointHandler)
        .WithSummary("Fremsøg cases")
        .WithDescription("Fremsøg cases ud fra URL-parametre.")
        .Produces<CaseDto[]>();

    private record GetCasesRequest(long? PodioItemId, Guid? FilArkivCaseId);

    private static async Task<IResult> EndpointHandler(
        [AsParameters] GetCasesRequest request,
        [FromServices] ICaseRepository caseRepository,
        CancellationToken cancellationToken)
    {
        var cases = await caseRepository.GetAll(request.PodioItemId, request.FilArkivCaseId);
        return Results.Ok(cases.ToDto());
    }
}