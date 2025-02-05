using FastEndpoints;
using FluentValidation;

namespace AktBob.Database.Endpoints.Tickets;

internal class PostTicketRequestValidator : Validator<PostTicketRequest>
{
    public PostTicketRequestValidator()
    {
        RuleFor(x => x.DeskproId).NotNull();
    }
}
