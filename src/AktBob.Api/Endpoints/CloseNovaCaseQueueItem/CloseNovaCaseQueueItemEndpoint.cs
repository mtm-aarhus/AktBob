using AktBob.Shared;
using AktBob.Shared.Jobs;
using FastEndpoints;

namespace AktBob.Api.Endpoints.CloseNovaCaseQueueItem;

internal class CloseNovaCaseQueueItemEndpoint : Endpoint<CloseNovaCaseQueueItemRequest>
{
    private readonly IJobDispatcher _jobDispatcher;

    public CloseNovaCaseQueueItemEndpoint(IJobDispatcher jobDispatcher)
    {
        _jobDispatcher = jobDispatcher;
    }

    public override void Configure()
    {
        Post("/Jobs/CloseNovaCase");
        Options(x => x.WithTags("Jobs"));
        Summary(s =>
        {
            s.Summary = "Creates a new OpenOrchestrator 'CloseNovaCase' queue item";
        });
    }

    public override async Task HandleAsync(CloseNovaCaseQueueItemRequest req, CancellationToken ct)
    {
        var job = new CreateCloseNovaCaseQueueItemJob(req.DeskproId);
        _jobDispatcher.Dispatch(job);
        await SendNoContentAsync();
    }
}
