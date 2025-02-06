using AktBob.ExternalQueue.UseCases.CreateQueueMessageCreateAktindsigtssag;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace AktBob.ExternalQueue.Endpoints;
internal class CreateAktindsigtssagAddQueueMessage : Endpoint<CreateAktindsigtssagAddQueueMessageRequest>
{
    private readonly IMediator _mediator;

    public CreateAktindsigtssagAddQueueMessage(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("GetOrganized/Aktindsigtssag");
        Options(x => x.WithTags("GetOrganized"));
    }

    public override async Task HandleAsync(CreateAktindsigtssagAddQueueMessageRequest req, CancellationToken ct)
    {
        var command = new CreateQueueMessageCreateAktindsigtssagCommand(req.DeskproTicketId, req.CaseTitle);
        var result = await _mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            await SendOkAsync(ct);
            return;
        }

        await SendErrorsAsync(cancellation: ct);
    }
}
