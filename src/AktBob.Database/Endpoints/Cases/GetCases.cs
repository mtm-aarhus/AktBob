using AktBob.Database.Contracts;
using AktBob.Database.Dtos;
using AktBob.Database.Extensions;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace AktBob.Database.Endpoints.Cases;

internal record GetCasesRequest(long? PodioItemId, Guid? FilArkivCaseId);

internal class GetCases(ICaseRepository caseRepository) : Endpoint<GetCasesRequest, IEnumerable<CaseDto>>
{
    private readonly ICaseRepository _caseRepository = caseRepository;

    public override void Configure()
    {
        Get("/Database/Cases");
        Options(x => x.WithTags("Database/Cases"));

        Description(x => x
            .Produces<IEnumerable<CaseDto>>(StatusCodes.Status200OK));
    }

    public override async Task HandleAsync(GetCasesRequest req, CancellationToken ct)
    {
        var cases = await _caseRepository.GetAll(req.PodioItemId, req.FilArkivCaseId);
        await SendOkAsync(cases.ToDto(), ct);
    }
}