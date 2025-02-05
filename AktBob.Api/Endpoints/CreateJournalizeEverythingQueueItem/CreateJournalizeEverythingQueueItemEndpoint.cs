using AktBob.Shared;
using AktBob.Shared.Contracts;
using FastEndpoints;

namespace AktBob.Api.Endpoints.CreateJournalizeEverythingQueueItem;

internal class CreateJournalizeEverythingQueueItemEndpoint(IJobDispatcher jobDispatcher) : Endpoint<CreateJournalizeEverythingQueueItemRequest>
{
    private readonly IJobDispatcher _jobDispatcher = jobDispatcher;

    public override void Configure()
    {
        Post("/Queue/JournalizeEverything");
        Options(x => x.WithTags("Queue"));
        Summary(s =>
        {
            s.Summary = "Creates queue item for journalizing everything";
        });
    }

    public override async Task HandleAsync(CreateJournalizeEverythingQueueItemRequest req, CancellationToken ct)
    {
        var job = new CreateJournalizeEverythingQueueItemJob(req.DeskproId);
        _jobDispatcher.Dispatch(job);
        await SendNoContentAsync(ct);
    }
}
