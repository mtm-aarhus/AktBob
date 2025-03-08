using AktBob.Shared;
using AktBob.Shared.Jobs;
using Ardalis.GuardClauses;
using FastEndpoints;

namespace AktBob.Api.Endpoints.PodioCase;

internal class PodioCaseEndpoint(IJobDispatcher jobDispatcher, IConfiguration configuration) : Endpoint<PodioCaseRequet>
{
    private readonly IJobDispatcher _jobDispatcher = jobDispatcher;
    private readonly IConfiguration _configuration = configuration;

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
        var appId = Guard.Against.Null(_configuration.GetValue<int?>("Podio:AktindsigtApp:Id"));
        var podioItemId = new PodioItemId(appId, req.PodioItemId);

        var job = new RegisterPodioCaseJob(podioItemId);
        _jobDispatcher.Dispatch(job);
        await SendNoContentAsync(ct);
    }
}
