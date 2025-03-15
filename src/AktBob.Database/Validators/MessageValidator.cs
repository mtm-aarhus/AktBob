using AktBob.Database.Entities;
using FluentValidation;

namespace AktBob.Database.Validators;

internal class MessageValidator : AbstractValidator<Message>
{
    public MessageValidator()
    {
        RuleFor(x => x.TicketId).NotEmpty();
        RuleFor(x => x.DeskproMessageId).NotEmpty();
    }
}