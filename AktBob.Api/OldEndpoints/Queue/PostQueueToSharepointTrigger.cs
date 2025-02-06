using AktBob.Shared;
using AktBob.Shared.Contracts;
using FastEndpoints;

namespace AktBob.ExternalQueue.Endpoints;
internal class PostQueueToSharepointTrigger(IJobDispatcher jobDispatcher) : Endpoint<PostQueueToSharepointTriggerRequest>
{
    private readonly string _queueIdentifier = "013bb1ee";
    private readonly IJobDispatcher _jobDispatcher = jobDispatcher;

    public override void Configure()
    {

        Post("/Queue/" + _queueIdentifier);
        AllowFormData(urlEncoded: true);
        Options(x => x.WithTags("Queue"));
        Summary(s =>
        {
            s.Summary = "FilArkiv to Sharepoint";
        });
    }

    public override async Task HandleAsync(PostQueueToSharepointTriggerRequest req, CancellationToken ct)
    {
        var job = new CreateToSharepointQueueItemJob(req.PodioItemId);
        _jobDispatcher.Dispatch(job);
        await SendNoContentAsync(ct);
    }
}
