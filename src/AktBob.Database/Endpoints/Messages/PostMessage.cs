using AktBob.Shared;
using AktBob.Shared.Jobs;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace AktBob.Database.Endpoints.Messages;

internal record PostMessageRequest(int DeskproTicketId);

internal record PostMessageResponse(int Id);

internal class PostMessage(IJobDispatcher jobDispatcher) : Endpoint<PostMessageRequest>
{
    private readonly IJobDispatcher _jobDispatcher = jobDispatcher;

    public override void Configure()
    {
        Post("/Database/Messages");
        Options(x => x.WithTags("Database/Messages"));

        Description(x => x
            .Produces(StatusCodes.Status201Created));
    }

    public override async Task HandleAsync(PostMessageRequest req, CancellationToken ct)
    {
        _jobDispatcher.Dispatch(new RegisterMessagesJob(req.DeskproTicketId), TimeSpan.FromSeconds(30));
        await SendNoContentAsync(ct);
    }
}