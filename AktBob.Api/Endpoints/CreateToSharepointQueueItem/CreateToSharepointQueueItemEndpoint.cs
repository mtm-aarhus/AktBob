using AktBob.Api.Endpoints.CreateDocumentListQueueItem;
using AktBob.Shared;
using AktBob.Shared.Contracts;
using FastEndpoints;

namespace AktBob.Api.Endpoints.CreateToSharepointQueueItem;

internal class CreateToSharepointQueueItemEndpoint(IJobDispatcher jobDispatcher) : Endpoint<CreateDocumentListQueueItemRequest>
{
    private readonly IJobDispatcher _jobDispatcher = jobDispatcher;

    public override void Configure()
    {

        Post("/Jobs/CreateToSharepointQueueItem", "/Queue/013bb1ee");
        AllowFormData(urlEncoded: true);
        Options(x => x.WithTags("Jobs"));
        Summary(s =>
        {
            s.Summary = "Creates a FilArkiv to Sharepoint queue item";
        });
    }

    public override async Task HandleAsync(CreateDocumentListQueueItemRequest req, CancellationToken ct)
    {
        var job = new CreateToSharepointQueueItemJob(req.PodioItemId);
        _jobDispatcher.Dispatch(job);
        await SendNoContentAsync(ct);
    }
}
