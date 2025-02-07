using AktBob.Shared;
using AktBob.Shared.Contracts;
using FastEndpoints;

namespace AktBob.Api.Endpoints.CreateDocumentListQueueItem;

internal class CreateDocumentListQueueItemEndpoint(IJobDispatcher jobDispatcher) : Endpoint<CreateDocumentListQueueItemRequest>
{
    private readonly IJobDispatcher _jobDispatcher = jobDispatcher;

    public override void Configure()
    {
        Post("/Jobs/CreateDocumentListQueueItem", "/Queue/f1dd04ad");
        AllowFormData(urlEncoded: true);
        Options(x => x.WithTags("Jobs"));
        Summary(s =>
        {
            s.Summary = "Creates document list queue item";
        });
    }

    public override async Task HandleAsync(CreateDocumentListQueueItemRequest req, CancellationToken ct)
    {
        var job = new CreateDocumentListQueueItemJob(req.PodioItemId);
        _jobDispatcher.Dispatch(job);
        await SendNoContentAsync(ct);
    }
}