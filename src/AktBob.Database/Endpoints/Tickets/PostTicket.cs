using AktBob.Database.Contracts.Dtos;
using AktBob.Database.Extensions;
using AktBob.Database.UseCases.Tickets;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace AktBob.Database.Endpoints.Tickets;

internal record PostTicketRequest(int DeskproId);

internal class PostTicketRequestValidator : Validator<PostTicketRequest>
{
    public PostTicketRequestValidator()
    {
        RuleFor(x => x.DeskproId).NotNull();
    }
}

internal class PostTicket(ICommandDispatcher commandDispatcher) : Endpoint<PostTicketRequest, TicketDto>
{
    private readonly ICommandDispatcher _commandDispatcher = commandDispatcher;

    public override void Configure()
    {
        Post("/Database/Tickets");
        Options(x => x.WithTags("Database/Tickets"));

        Description(x => x
          .Produces<TicketDto>(StatusCodes.Status201Created)
          .ProducesProblem(StatusCodes.Status400BadRequest));
    }

    public override async Task HandleAsync(PostTicketRequest req, CancellationToken ct)
    {
        var command = new AddTicketCommand(req.DeskproId);
        var result = await _commandDispatcher.Dispatch(command);

        if (result.IsSuccess)
        {
            await SendCreatedAtAsync<GetTicket>(routeValues: null, responseBody: result.Value, cancellation: ct);
            return;
        }

        await this.SendResponse(result, r => r.Value);
    }
}
