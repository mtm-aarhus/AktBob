using AktBob.Database.Contracts;
using AktBob.Database.Dtos;
using AktBob.Database.Extensions;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace AktBob.Database.Endpoints.Cases;

internal record PatchCaseRequest
{
    public int Id { get; set; }
    public long? PodioItemId { get; set; }
    public string? CaseNumber { get; set; }
    public Guid? FilArkivCaseId { get; set; }
    public string? SharepointFolderName { get; set; }

}

internal class PatchCase(ICaseRepository caseRepository) : Endpoint<PatchCaseRequest, CaseDto>
{
    private readonly ICaseRepository _caseRepository = caseRepository;

    public override void Configure()
    {
        Patch("/Database/Cases/{Id}");
        Options(x => x.WithTags("Database/Cases"));

        Description(x => x
            .Produces<CaseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound));
    }

    public override async Task HandleAsync(PatchCaseRequest req, CancellationToken ct)
    {
        // Get existing case from repository
        var @case = await _caseRepository.Get(req.Id);

        if (@case == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        // Update case properties
        if (!string.IsNullOrEmpty(req.CaseNumber))
        {
            @case.CaseNumber = req.CaseNumber;
        }

        if (!string.IsNullOrEmpty(req.SharepointFolderName))
        {
            @case.SharepointFolderName = req.SharepointFolderName;
        }

        @case.PodioItemId = req.PodioItemId ?? @case.PodioItemId;
        @case.FilArkivCaseId = req.FilArkivCaseId ?? @case.FilArkivCaseId;


        // Update entity
        var updated = await _caseRepository.Update(@case) == 1;


        // Response
        if (updated)
        {
            var updatedCase = await _caseRepository.Get(req.Id);

            if (updatedCase == null)
            {
                await SendErrorsAsync(500, ct);
                return;
            }

            await SendOkAsync(updatedCase.ToDto(), ct);
            return;
        }

        await SendErrorsAsync(500, ct);
    }
}
