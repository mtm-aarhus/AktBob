using AktBob.Database.Contracts;
using AktBob.Database.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace AktBob.Api.Endpoints.Cases;

internal static class GetCases
{
    public static string Description => "Fremsøg cases ud fra URL-parametre.";
    public static string Summery => "Fremsøg cases";
    
    internal record GetCasesRequest(long? PodioItemId, Guid? FilArkivCaseId);

    public static async Task<IResult> Endpoint(
        [AsParameters] GetCasesRequest request,
        [FromServices] ICaseRepository caseRepository,
        CancellationToken cancellationToken)
    {
        var cases = await caseRepository.GetAll(request.PodioItemId, request.FilArkivCaseId);
        return Results.Ok(cases.ToDto());
    }
}