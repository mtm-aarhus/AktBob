using AktBob.Shared;
using AktBob.Shared.Contracts;
using FastEndpoints;

namespace AktBob.ExternalQueue.Endpoints;

internal class PostQueueJournalizeEverythingTrigger(IJobDispatcher jobDispatcher) : Endpoint<PostQueueJournalizeEverythingTriggerRequest>
{
    private readonly string _queueIdentifier = "babf56f3";
    private readonly IJobDispatcher _jobDispatcher = jobDispatcher;

    public override void Configure()
    {
        Post("/Queue/" + _queueIdentifier);
        Options(x => x.WithTags("Queue"));
        Summary(s =>
        {
            s.Summary = "Journalize everything";
        });
    }

    public override async Task HandleAsync(PostQueueJournalizeEverythingTriggerRequest req, CancellationToken ct)
    {
        var job = new CreateJournalizeEverythingQueueItemJob(req.DeskproTicketId);
        _jobDispatcher.Dispatch(job);
        await SendNoContentAsync(ct);
    }
}
