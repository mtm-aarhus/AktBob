using AktBob.Shared;
using AktBob.Shared.Contracts;
using FastEndpoints;

namespace AktBob.Api.Endpoints.PodioCase;

internal class PodioCaseEndpoint(IJobDispatcher jobDispatcher) : Endpoint<PodioCaseRequet>
{
    private readonly IJobDispatcher _jobDispatcher = jobDispatcher;

    public override void Configure()
    {

        Post("/Jobs/PodioCase");
        Options(x => x.WithTags("Jobs"));
        AllowFormData(urlEncoded: true);
        Summary(s =>
        {
            s.Summary = "Initiates a job that eventually registers the Podio case in the database";
        });
    }

    public override async Task HandleAsync(PodioCaseRequet req, CancellationToken ct)
    {
        var job = new RegisterPodioCaseJob(req.PodioItemId);
        _jobDispatcher.Dispatch(job);
        await SendNoContentAsync(ct);
    }
}
