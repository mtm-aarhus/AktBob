using AktBob.Api.Endpoints.DocumentListQueueItem;
using AktBob.Shared;
using AktBob.Shared.Contracts;
using FastEndpoints;

namespace AktBob.Api.Endpoints.ToSharepointQueueItem;

internal class ToSharepointQueueItemEndpoint(IJobDispatcher jobDispatcher) : Endpoint<DocumentListQueueItemRequest>
{
    private readonly IJobDispatcher _jobDispatcher = jobDispatcher;

    public override void Configure()
    {

        Post("/Jobs/ToSharepointQueueItem");
        AllowFormData(urlEncoded: true);
        Options(x => x.WithTags("Jobs"));
        Summary(s =>
        {
            s.Summary = "Creates a new OpenOrchestrator 'AktbobFromFilarkivToSharePoint' queue item";
        });
    }

    public override async Task HandleAsync(DocumentListQueueItemRequest req, CancellationToken ct)
    {
        var job = new CreateToSharepointQueueItemJob(req.PodioItemId);
        _jobDispatcher.Dispatch(job);
        await SendNoContentAsync(ct);
    }
}
