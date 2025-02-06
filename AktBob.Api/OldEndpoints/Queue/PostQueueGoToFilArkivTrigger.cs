using AktBob.ExternalQueue.Endpoints;
using AktBob.Shared;
using AktBob.Shared.Contracts;
using FastEndpoints;

namespace AktBob.Api.OldEndpoints.Queue;
internal class PostQueueGoToFilArkivTrigger(IJobDispatcher jobDispatcher) : Endpoint<PostQueueGoToFilArkivTriggerRequest>
{
    private readonly string _queueIdentifier = "edbae766";
    private readonly IJobDispatcher _jobDispatcher = jobDispatcher;

    public override void Configure()
    {
        Post("/Queue/" + _queueIdentifier);
        AllowFormData(urlEncoded: true);
        Options(x => x.WithTags("Queue"));
        Summary(s =>
        {
            s.Summary = "GetOrganized to FilArkiv";
        });
    }

    public override async Task HandleAsync(PostQueueGoToFilArkivTriggerRequest req, CancellationToken ct)
    {
        var job = new CreateGoToFilArkivQueueItemJob(req.PodioItemId);
        _jobDispatcher.Dispatch(job);
        await SendNoContentAsync(ct);
    }
}
