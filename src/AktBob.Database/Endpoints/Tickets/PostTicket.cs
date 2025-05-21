using AktBob.Database.Dtos;
using AktBob.Shared;
using AktBob.Shared.Jobs;
using AktBob.Shared.Types.Deskpro;
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

internal class PostTicket(IJobDispatcher jobDispatcher) : Endpoint<PostTicketRequest, TicketDto>
{
    private readonly IJobDispatcher _jobDispatcher = jobDispatcher;

    public override void Configure()
    {
        Post("/Database/Tickets");
        Options(x => x.WithTags("Database/Tickets"));

        Description(x => x.Produces<TicketDto>(StatusCodes.Status204NoContent));
    }

    public override async Task HandleAsync(PostTicketRequest req, CancellationToken ct)
    {
        var ticketId = TicketId.Create(req.DeskproId);
        var job = new RegisterDeskproTicketJob(ticketId);
        _jobDispatcher.Dispatch(job);
        await SendNoContentAsync(ct);
    }
}
