using AktBob.Shared;
using AktBob.Shared.Contracts;
using FastEndpoints;

namespace AktBob.Api.Endpoints.CreateDocumentListQueueItem;

internal class CreateDocumentListQueueItemEndpoint : Endpoint<CreateDocumentListQueueItemRequest>
{
    private readonly IJobDispatcher _jobDispatcher;

    public CreateDocumentListQueueItemEndpoint(IJobDispatcher jobDispatcher)
    {
        _jobDispatcher = jobDispatcher;
    }

    public override void Configure()
    {
        Post("/Queue/DocumentList");
        AllowFormData(urlEncoded: true);
        Options(x => x.WithTags("Queue"));
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