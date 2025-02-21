using AktBob.Shared;
using AktBob.Shared.Contracts;
using FastEndpoints;

namespace AktBob.Api.Endpoints.RegisterPodioCase;

internal class RegisterPodioCaseEndpoint(IJobDispatcher jobDispatcher) : Endpoint<RegisterPodioCaseRequet>
{
    private readonly IJobDispatcher _jobDispatcher = jobDispatcher;

    public override void Configure()
    {

        Post("/Jobs/RegisterPodioCase");
        Options(x => x.WithTags("Jobs"));
        AllowFormData(urlEncoded: true);
        Summary(s =>
        {
            s.Summary = "Registers Podio case in database";
        });
    }

    public override async Task HandleAsync(RegisterPodioCaseRequet req, CancellationToken ct)
    {
        var job = new RegisterPodioCaseJob(req.PodioItemId);
        _jobDispatcher.Dispatch(job);
        await SendNoContentAsync(ct);
    }
}
