using AktBob.Api.Endpoints.DocumentListQueueItem;
using AktBob.Shared;
using AktBob.Shared.Jobs;
using Ardalis.GuardClauses;
using FastEndpoints;

namespace AktBob.Api.Endpoints.ToFilArkivQueueItem;

internal class ToFilArkivQueueItemEndpoint(IJobDispatcher jobDispatcher, IConfiguration configuration) : Endpoint<DocumentListQueueItemRequest>
{
    private readonly IJobDispatcher _jobDispatcher = jobDispatcher;
    private readonly IConfiguration _configuration = configuration;

    public override void Configure()
    {
        Post("/Jobs/ToFilArkivQueueItem");
        AllowFormData(urlEncoded: true);
        Options(x => x.WithTags("Jobs"));
        Summary(s =>
        {
            s.Summary = "Creates a new OpenOrchestrator 'AktbobGenererAktindsigter' queue item";
        });
    }

    public override async Task HandleAsync(DocumentListQueueItemRequest req, CancellationToken ct)
    {
        var appId = Guard.Against.Null(_configuration.GetValue<int?>("Podio:AktindsigtApp:Id"));
        var podioItemId = new PodioItemId(appId, req.PodioItemId);

        var job = new CreateGoToFilArkivQueueItemJob(podioItemId);
        _jobDispatcher.Dispatch(job);
        await SendNoContentAsync(ct);
    }
}