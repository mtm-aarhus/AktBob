using AktBob.Shared;
using AktBob.Shared.Contracts;
using FastEndpoints;

namespace AktBob.Api.Endpoints.RegisterPodioCase;

internal class RegisterPodioCaseEndpoint(IJobDispatcher jobDispatcher) : Endpoint<RegisterPodioCaseRequet>
{
    private readonly IJobDispatcher _jobDispatcher = jobDispatcher;

    public override void Configure()
    {

        Post("/Database/Cases/RegisterPodioCase");
        Options(x => x.WithTags("Database/Cases"));
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
