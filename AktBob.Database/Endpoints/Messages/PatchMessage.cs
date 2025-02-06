using AktBob.Database.Contracts;
using AktBob.Database.Contracts.Dtos;
using FastEndpoints;
using MassTransit;
using MassTransit.Mediator;
using Microsoft.AspNetCore.Http;

namespace AktBob.Database.Endpoints.Messages;

internal record PatchMessageRequest(int Id, int? GoDocumentId);

internal class PatchMessage(IMediator mediator) : Endpoint<PatchMessageRequest, MessageDto>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Patch("/Database/Messages/{id}");
        Options(x => x.WithTags("Database/Messages"));

        Description(x => x
            .Produces<MessageDto>(StatusCodes.Status200OK));

        Summary(x =>
        {
            x.Description = "Opdaterer en specific message i databasen. Pt. kan kun opdateres GetOrganized dokument ID. Returnerer message-objektet fra databasen.";
        });
    }

    public override async Task HandleAsync(PatchMessageRequest req, CancellationToken ct)
    {
        var command = new UpdateMessageCommand(req.Id, req.GoDocumentId);
        var result = await _mediator.SendRequest(command, ct);

        if (result.Status == Ardalis.Result.ResultStatus.NotFound)
        {
            await SendNotFoundAsync();
            return;
        }

        if (!result.IsSuccess)
        {
            await SendErrorsAsync();
            return;
        }

        var dto = result.Value;

        await SendOkAsync(dto);
    }
}
