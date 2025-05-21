using FastEndpoints;
using FluentValidation;

namespace AktBob.Api.Endpoints.RegisterOS2FormsSubmission;

internal class RegisterOS2FormsSubmissionRequestValidator : Validator<RegisterOS2FormsSubmissionRequest>
{
    public RegisterOS2FormsSubmissionRequestValidator()
    {
        RuleFor(x => x.DeskproTicketId).NotEmpty().GreaterThan(0);
        RuleFor(x => x.OS2FormsSubmissionId).NotEmpty();
    }
}
