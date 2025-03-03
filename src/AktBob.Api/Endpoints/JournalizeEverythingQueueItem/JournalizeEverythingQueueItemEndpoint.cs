using AktBob.Shared;
using AktBob.Shared.Jobs;
using FastEndpoints;

namespace AktBob.Api.Endpoints.JournalizeEverythingQueueItem;

internal class JournalizeEverythingQueueItemEndpoint(IJobDispatcher jobDispatcher) : Endpoint<JournalizeEverythingQueueItemRequest>
{
    private readonly IJobDispatcher _jobDispatcher = jobDispatcher;

    public override void Configure()
    {
        Post("/Jobs/JournalizeEverythingQueueItem");
        Options(x => x.WithTags("Jobs"));
        Summary(s =>
        {
            s.Summary = "Creates a new OpenOrchestrator 'AktbobJournaliser' queue item";
        });
    }

    public override async Task HandleAsync(JournalizeEverythingQueueItemRequest req, CancellationToken ct)
    {
        var job = new CreateJournalizeEverythingQueueItemJob(req.DeskproId);
        _jobDispatcher.Dispatch(job);
        await SendNoContentAsync(ct);
    }
}
