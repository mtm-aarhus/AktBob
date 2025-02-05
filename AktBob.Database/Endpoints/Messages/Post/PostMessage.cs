using AktBob.Database.UseCases.Messages.PostMessage;
using FastEndpoints;
using MassTransit.Mediator;
using Microsoft.AspNetCore.Http;

namespace AktBob.Database.Endpoints.Messages.Post;
internal class PostMessage(IMediator mediator) : Endpoint<PostMessageRequest>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Post("/Database/Messages");
        Options(x => x.WithTags("Database/Messages"));

        Description(x => x
            .Produces(StatusCodes.Status201Created));

        Summary(x =>
        {
            x.Description = "Tilføjer køelement til baggrundsproces, der opretter en message i databasen.";
        });
    }

    public override async Task HandleAsync(PostMessageRequest req, CancellationToken ct)
    {
        await SendOkAsync(ct);

        var postMessageCommand = new PostMessageCommand(req.DeskproTicketId);
        await _mediator.Send(postMessageCommand);

        await SendCreatedAtAsync<GetMessages>(routeValues: null, responseBody: null, cancellation: ct);
    }
}
