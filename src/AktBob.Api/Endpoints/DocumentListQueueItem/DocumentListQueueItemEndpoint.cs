using AktBob.Shared;
using AktBob.Shared.Jobs;
using FastEndpoints;

namespace AktBob.Api.Endpoints.DocumentListQueueItem;

internal class DocumentListQueueItemEndpoint(IJobDispatcher jobDispatcher) : Endpoint<DocumentListQueueItemRequest>
{
    private readonly IJobDispatcher _jobDispatcher = jobDispatcher;

    public override void Configure()
    {
        Post("/Jobs/DocumentListQueueItem");
        AllowFormData(urlEncoded: true);
        Options(x => x.WithTags("Jobs"));
        Summary(s =>
        {
            s.Summary = "Creates a new OpenOrchestrator 'AktbobDokumentlisteQueue' queue item";
        });
    }

    public override async Task HandleAsync(DocumentListQueueItemRequest req, CancellationToken ct)
    {
        var job = new CreateDocumentListQueueItemJob(req.PodioItemId);
        _jobDispatcher.Dispatch(job);
        await SendNoContentAsync(ct);
    }
}