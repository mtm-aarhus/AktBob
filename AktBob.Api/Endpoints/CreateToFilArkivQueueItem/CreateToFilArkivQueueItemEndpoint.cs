using AktBob.Api.Endpoints.CreateDocumentListQueueItem;
using AktBob.Shared;
using AktBob.Shared.Contracts;
using FastEndpoints;

namespace AktBob.Api.Endpoints.CreateGoToFilArkivQueueItem;

internal class CreateToFilArkivQueueItemEndpoint : Endpoint<CreateDocumentListQueueItemRequest>
{
    private readonly IJobDispatcher _jobDispatcher;

    public CreateToFilArkivQueueItemEndpoint(IJobDispatcher jobDispatcher)
    {
        _jobDispatcher = jobDispatcher;
    }

    public override void Configure()
    {
        Post("/Jobs/CreateToFilArkivQueueItem", "/Queue/edbae766");
        AllowFormData(urlEncoded: true);
        Options(x => x.WithTags("Jobs"));
        Summary(s =>
        {
            s.Summary = "Creates To-FilArkiv queue item";
        });
    }

    public override async Task HandleAsync(CreateDocumentListQueueItemRequest req, CancellationToken ct)
    {
        var job = new CreateGoToFilArkivQueueItemJob(req.PodioItemId);
        _jobDispatcher.Dispatch(job);
        await SendNoContentAsync(ct);
    }
}