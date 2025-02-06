using AktBob.Shared.Contracts;
using AktBob.Shared;
using FastEndpoints;
using AktBob.ExternalQueue.Endpoints;

namespace AktBob.Api.OldEndpoints.Queue;
internal class PostQueueDocumentListTrigger(IJobDispatcher jobDispatcher) : Endpoint<PostQueueDocumentListTriggerRequest>
{
    private readonly string _queueIdentifier = "f1dd04ad";
    private readonly IJobDispatcher _jobDispatcher = jobDispatcher;

    public override void Configure()
    {
        Post("/Queue/" + _queueIdentifier);
        AllowFormData(urlEncoded: true);
        Options(x => x.WithTags("Queue"));
        Summary(s =>
        {
            s.Summary = "Generate document list";
        });
    }

    public override async Task HandleAsync(PostQueueDocumentListTriggerRequest req, CancellationToken ct)
    {
        var job = new CreateDocumentListQueueItemJob(req.PodioItemId);
        _jobDispatcher.Dispatch(job);
        await SendNoContentAsync(ct);
    }
}
