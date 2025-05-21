using AktBob.Shared;
using AktBob.Shared.Jobs;
using AktBob.Shared.Types.Deskpro;
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
        var ticketId = TicketId.Create(req.DeskproId); 
        var job = new CreateJournalizeEverythingQueueItemJob(ticketId);
        _jobDispatcher.Dispatch(job);
        await SendNoContentAsync(ct);
    }
}
