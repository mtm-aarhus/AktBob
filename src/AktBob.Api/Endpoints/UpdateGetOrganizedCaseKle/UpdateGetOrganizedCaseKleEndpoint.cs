using AktBob.Shared;
using AktBob.Shared.Contracts.Processors.GetOrganized;
using FastEndpoints;

namespace AktBob.Api.Endpoints.UpdateGetOrganizedCaseKle;

internal class UpdateGetOrganizedCaseKleEndpoint : Endpoint<UpdateGetOrganizedCaseKleRequest>
{
    private readonly IJobDispatcher _jobDispatcher;

    public UpdateGetOrganizedCaseKleEndpoint(IJobDispatcher jobDispatcher)
    {
        _jobDispatcher = jobDispatcher;
    }

    public override void Configure()
    {
        Post("/Jobs/UpdateGetOrganizedCaseKle");
        Options(x => x.WithTags("Jobs"));
        Summary(s =>
        {
            s.Summary = "Sets KLE attribute on the target GetOrganized case based on the source case.";
        });
    }

    public override async Task HandleAsync(UpdateGetOrganizedCaseKleRequest req, CancellationToken ct)
    {
        var job = new UpdateGetOrganizedCaseSetKleValueJob(req.TargetCaseId, req.SourceCaseId);
        _jobDispatcher.Dispatch(job);
        await SendNoContentAsync(ct);
    }
}
