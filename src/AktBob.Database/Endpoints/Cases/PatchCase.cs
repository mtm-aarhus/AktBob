using AktBob.Database.Dtos;
using AktBob.Shared;
using AktBob.Shared.Contracts.Database;
using AktBob.Shared.Jobs;
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

internal class PatchCase(IJobDispatcher jobDispatcher) : Endpoint<PatchCaseRequest, CaseDto>
{
    private readonly IJobDispatcher _jobDispatcher = jobDispatcher;

    public override void Configure()
    {
        Patch("/Database/Cases/{Id}");
        Options(x => x.WithTags("Database/Cases"));

        Description(x => x.Produces<CaseDto>(StatusCodes.Status204NoContent));
    }

    public override async Task HandleAsync(PatchCaseRequest req, CancellationToken ct)
    {
        var job = new UpdateDatabaseCaseJob(
            Id: req.Id, 
            PodioItemId: req.PodioItemId,
            CaseNumber: req.CaseNumber, 
            FilArkivCaseId: req.FilArkivCaseId, 
            SharepointFolderName: req.SharepointFolderName);

        _jobDispatcher.Dispatch(job);
        await SendNoContentAsync(ct);
    }
}
