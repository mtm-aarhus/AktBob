using AktBob.Shared;
using AktBob.Shared.Jobs;
using AktBob.Shared.Types.Deskpro;
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
        var ticketId = TicketId.Create(req.DeskproId);
        var job = new CreateCloseNovaCaseQueueItemJob(ticketId);
        _jobDispatcher.Dispatch(job);
        await SendNoContentAsync();
    }
}
