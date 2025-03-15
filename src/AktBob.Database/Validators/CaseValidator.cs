using AktBob.Database.Entities;
using FluentValidation;

namespace AktBob.Database.Validators;

internal class CaseValidator : AbstractValidator<Case>
{
    public CaseValidator()
    {
        RuleFor(x => x.TicketId).NotEmpty();
        RuleFor(x => x.PodioItemId).NotEmpty();
        RuleFor(x => x.CaseNumber).NotEmpty();
    }
}
