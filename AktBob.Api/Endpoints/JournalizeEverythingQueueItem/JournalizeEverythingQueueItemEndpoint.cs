using AktBob.Shared;
using AktBob.Shared.Contracts;
using FastEndpoints;

namespace AktBob.Api.Endpoints.JournalizeEverythingQueueItem;

internal class JournalizeEverythingQueueItemEndpoint(IJobDispatcher jobDispatcher) : Endpoint<JournalizeEverythingQueueItemRequest>
{
    private readonly IJobDispatcher _jobDispatcher = jobDispatcher;

    public override void Configure()
    {
        Post("/Jobs/JournalizeEverythingQueueItem", "/Jobs/CreateJournalizeEverythingQueueItem");
        Options(x => x.WithTags("Jobs"));
        Summary(s =>
        {
            s.Summary = "Creates queue item for journalizing everything";
        });
    }

    public override async Task HandleAsync(JournalizeEverythingQueueItemRequest req, CancellationToken ct)
    {
        var job = new CreateJournalizeEverythingQueueItemJob(req.DeskproId);
        _jobDispatcher.Dispatch(job);
        await SendNoContentAsync(ct);
    }
}
