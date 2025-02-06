using AktBob.Shared.Contracts;
using AktBob.Shared;
using FastEndpoints;
using AktBob.ExternalQueue.Endpoints;

namespace AktBob.Api.OldEndpoints.Queue;
internal class CheckOCRScreeningStatusAddCase(IJobDispatcher jobDispatcher) : Endpoint<AddCaseRequest>
{
    private readonly IJobDispatcher _jobDispatcher = jobDispatcher;

    public override void Configure()
    {
        Post("CheckOCRScreeningStatus/Case");
        Options(x => x.WithTags("CheckOCRScreeningStatus"));
    }

    public override async Task HandleAsync(AddCaseRequest req, CancellationToken ct)
    {
        var job = new CheckOCRScreeningStatusJob(req.FilArkivCaseId, req.PodioItemId);
        _jobDispatcher.Dispatch(job);
        await SendNoContentAsync();
    }
}
