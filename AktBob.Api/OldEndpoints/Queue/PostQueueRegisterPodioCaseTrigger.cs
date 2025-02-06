using AktBob.Shared;
using AktBob.Shared.Contracts;
using FastEndpoints;

namespace AktBob.ExternalQueue.Endpoints;
internal class PostQueueRegisterPodioCaseTrigger(IJobDispatcher jobDispatcher) : Endpoint<PostQueueRegisterPodioCaseTriggerRequest>
{
    private readonly string _queueIdentifier = "e437047b";
    private readonly IJobDispatcher _jobDispatcher = jobDispatcher;

    public override void Configure()
    {
        Post("/Queue/" + _queueIdentifier);
        AllowFormData(urlEncoded: true);
        Options(x => x.WithTags("Queue"));
        Summary(s =>
        {
            s.Summary = "Register Podio case";
        });
    }

    public override async Task HandleAsync(PostQueueRegisterPodioCaseTriggerRequest req, CancellationToken ct)
    {
        var job = new RegisterPodioCaseJob(req.PodioItemId);
        _jobDispatcher.Dispatch(job);
        await SendNoContentAsync(ct);
    }
}
