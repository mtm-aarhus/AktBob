using AktBob.Database.UseCases.Messages.DeleteMessage;
using FastEndpoints;
using MassTransit.Mediator;
using Microsoft.AspNetCore.Http;

namespace AktBob.Database.Endpoints.Messages.Delete;

internal class DeleteMessage(IMediator mediator) : Endpoint<DeleteMessageRequest>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Delete("/Database/Messages/{id}");
        Options(x => x.WithTags("Database/Messages"));

        Description(x => x
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound));

        Summary(x =>
        {
            x.Description = "Sletter en message i databasen (soft delete).";
        });
    }

    public override async Task HandleAsync(DeleteMessageRequest req, CancellationToken ct)
    {
        var command = new DeleteMessageCommand(req.Id);
        await _mediator.Send(command, ct);
        await SendNoContentAsync(ct);
    }
}
