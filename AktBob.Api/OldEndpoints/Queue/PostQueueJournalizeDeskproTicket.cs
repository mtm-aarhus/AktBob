using AktBob.Shared;
using AktBob.Shared.Contracts;
using FastEndpoints;

namespace AktBob.ExternalQueue.Endpoints;
internal class PostQueueJournalizeDeskproTicket(IJobDispatcher jobDispatcher) : Endpoint<PostQueueJournalizeDeskproTicketRequest>
{
    private readonly IJobDispatcher _jobDispatcher = jobDispatcher;
    private readonly string _queueIdentifier = "6inqa7a8";

    public override void Configure()
    {
        Post("/Queue/" + _queueIdentifier);
        Options(x => x.WithTags("Queue"));
        Summary(s =>
        {
            s.Summary = "Journalize Deskpro ticket";
        });
    }

    public override async Task HandleAsync(PostQueueJournalizeDeskproTicketRequest req, CancellationToken ct)
    {
        var job = new CreateJournalizeEverythingQueueItemJob(req.TicketId);
        _jobDispatcher.Dispatch(job);
        await SendNoContentAsync(ct);
    }
}
