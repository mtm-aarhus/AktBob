using AktBob.Shared;
using AktBob.Shared.Contracts;
using FastEndpoints;

namespace AktBob.Api.Endpoints.CheckOCRScreeningStatus;

internal class CheckOCRScreeningEndpoint(IJobDispatcher jobDispatcher) : Endpoint<CheckOCRScreeningRequest>
{
    private readonly IJobDispatcher _jobDispatcher = jobDispatcher;

    public override void Configure()
    {
        Post("/Jobs/CheckOCRScreeningStatus", "/CheckOCRScreeningStatus/Case");
        Options(x => x.WithTags("Jobs"));
        Summary(s =>
        {
            s.Summary = "Initiate a new job checking the OCR screening status for a FilArkiv case's documents";
        });
    }

    public override async Task HandleAsync(CheckOCRScreeningRequest req, CancellationToken ct)
    {
        var job = new CheckOCRScreeningStatusJob(req.FilArkivCaseId, req.PodioItemId);
        _jobDispatcher.Dispatch(job);
        await SendNoContentAsync();
    }
}
