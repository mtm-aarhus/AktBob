using AktBob.Api.Endpoints.DocumentListQueueItem;
using AktBob.Shared;
using AktBob.Shared.Contracts;
using FastEndpoints;

namespace AktBob.Api.Endpoints.ToFilArkivQueueItem;

internal class ToFilArkivQueueItemEndpoint(IJobDispatcher jobDispatcher) : Endpoint<DocumentListQueueItemRequest>
{
    private readonly IJobDispatcher _jobDispatcher = jobDispatcher;

    public override void Configure()
    {
        Post("/Jobs/ToFilArkivQueueItem", "/Jobs/CreateToFilArkivQueueItem");
        AllowFormData(urlEncoded: true);
        Options(x => x.WithTags("Jobs"));
        Summary(s =>
        {
            s.Summary = "Creates To-FilArkiv queue item";
        });
    }

    public override async Task HandleAsync(DocumentListQueueItemRequest req, CancellationToken ct)
    {
        var job = new CreateGoToFilArkivQueueItemJob(req.PodioItemId);
        _jobDispatcher.Dispatch(job);
        await SendNoContentAsync(ct);
    }
}