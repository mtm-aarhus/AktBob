using AktBob.Shared.Contracts;
using AktBob.Shared;
using FastEndpoints;

namespace AktBob.Api.Endpoints.CreateAfgørelsesskrivelseQueueItem;

internal class CreateAfgørelsesskrivelseQueueItemEndpoint(IJobDispatcher jobDispatcher) : Endpoint<CreateAfgørelsesskrivelseQueueItemRequest>
{
    private readonly IJobDispatcher _jobDispatcher = jobDispatcher;

    public override void Configure()
    {
        Post("/Jobs/CreateAfgoerelsesskrivelseQueueItem");
        Options(x => x.WithTags("Jobs"));
        Summary(s =>
        {
            s.Summary = "Creates afgørelsesskrivelse queue item";
        });
    }

    public override async Task HandleAsync(CreateAfgørelsesskrivelseQueueItemRequest req, CancellationToken ct)
    {
        var job = new CreateAfgørelsesskrivelseQueueItemJob(req.DeskproId);
        _jobDispatcher.Dispatch(job);
        await SendNoContentAsync(ct);
    }
}