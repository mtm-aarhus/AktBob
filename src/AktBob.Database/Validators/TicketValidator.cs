using AktBob.Database.Entities;
using FluentValidation;

namespace AktBob.Database.Validators;

internal class TicketValidator : AbstractValidator<Ticket>
{
    public TicketValidator()
    {
        RuleFor(x => x.DeskproId).NotEmpty();
    }
}
