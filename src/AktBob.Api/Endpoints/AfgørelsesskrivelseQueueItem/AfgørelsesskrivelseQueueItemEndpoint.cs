using AktBob.Shared.Jobs;
using AktBob.Shared;
using FastEndpoints;
using AktBob.Api.Endpoints.CreateAfgørelsesskrivelseQueueItem;
using AktBob.Shared.Types.Deskpro;

namespace AktBob.Api.Endpoints.AfgørelsesskrivelseQueueItem;

internal class AfgørelsesskrivelseQueueItemEndpoint(IJobDispatcher jobDispatcher) : Endpoint<AfgørelsesskrivelseQueueItemRequest>
{
    private readonly IJobDispatcher _jobDispatcher = jobDispatcher;

    public override void Configure()
    {
        Post("/Jobs/AfgoerelsesskrivelseQueueItem");
        Options(x => x.WithTags("Jobs"));
        Summary(s =>
        {
            s.Summary = "Creates a new OpenOrchestrator 'AktbobAfgørelse' queue item";
        });
    }

    public override async Task HandleAsync(AfgørelsesskrivelseQueueItemRequest req, CancellationToken ct)
    {
        var ticketId = TicketId.Create(req.DeskproId);
        var job = new CreateAfgørelsesskrivelseQueueItemJob(ticketId);
        _jobDispatcher.Dispatch(job);
        await SendNoContentAsync(ct);
    }
}