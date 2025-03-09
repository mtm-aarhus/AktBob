using AktBob.Database.Contracts;
using AktBob.Database.Dtos;
using AktBob.Database.Entities;
using AktBob.Database.Extensions;
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

internal class PostTicket(ITicketRepository ticketRepository) : Endpoint<PostTicketRequest, TicketDto>
{
    private readonly ITicketRepository _ticketRepository = ticketRepository;

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
        var ticket = new Ticket
        {
            DeskproId = req.DeskproId,
        };

        var success = await _ticketRepository.Add(ticket);

        if (!success)
        {
            await SendErrorsAsync(500, ct);
            return;
        }

        await SendOkAsync(ticket.ToDto(), ct);
    }
}
