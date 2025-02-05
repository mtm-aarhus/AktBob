using AktBob.Database.Contracts.Dtos;
using AktBob.Database.Extensions;
using AktBob.Database.UseCases.Messages.GetMessageByDeskproMessageId;
using AktBob.Database.UseCases.Messages.GetMessages;
using FastEndpoints;
using MassTransit;
using MassTransit.Mediator;
using Microsoft.AspNetCore.Http;

namespace AktBob.Database.Endpoints.Messages;
internal class GetMessages(IMediator mediator) : Endpoint<GetMessagesRequest, IEnumerable<MessageDto>>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("/Database/Messages");
        Options(x => x.WithTags("Database/Messages"));

        Description(x => x
            .Produces<IEnumerable<MessageDto>>(StatusCodes.Status200OK));

        Summary(x =>
        {
            x.Description = "Henter messages i databasen.";
        });
    }

    public override async Task HandleAsync(GetMessagesRequest req, CancellationToken ct)
    {
        var includeJournalized = req.IncludeJournalized ?? false;

        if (req.DeskproMessageId is not null)
        {
            var getMessageByDeskproMessageIdQuery = new GetMessageByDeskproMessageIdQuery((int)req.DeskproMessageId);
            var getMessageByDeskproMessageIdResult = await _mediator.SendRequest(getMessageByDeskproMessageIdQuery, ct);

            if (getMessageByDeskproMessageIdResult.IsSuccess)
            {
                var dto = getMessageByDeskproMessageIdResult.Value.Select(m => m.ToDto());
                await SendOkAsync(dto, ct);
                return;
            }

            await SendNotFoundAsync();
            return;
        }

        var query = new GetMessagesQuery(includeJournalized);
        var result = await _mediator.SendRequest(query, ct);

        if (result.IsSuccess)
        {
            var dto = result.Value.Select(m => m.ToDto());
            await SendOkAsync(dto, ct);
            return;
        }
        
        await SendErrorsAsync();
    }
}
