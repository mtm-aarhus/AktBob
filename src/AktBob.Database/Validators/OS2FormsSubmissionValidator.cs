using AktBob.Database.Entities;
using FluentValidation;

namespace AktBob.Database.Validators;
internal class OS2FormsSubmissionValidator : AbstractValidator<OS2FormsSubmission>
{
    public OS2FormsSubmissionValidator()
    {
        RuleFor(x => x.DeskproTicketId).NotEmpty();
        RuleFor(x => x.SubmissionId).NotEmpty();
    }
}
