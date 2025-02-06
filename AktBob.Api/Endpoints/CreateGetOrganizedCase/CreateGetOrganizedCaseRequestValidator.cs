using FastEndpoints;
using FluentValidation;

namespace AktBob.ExternalQueue.Endpoints;
internal class CreateGetOrganizedCaseRequestValidator : Validator<CreateGetOrganizedCaseRequest>
{
    public CreateGetOrganizedCaseRequestValidator()
    {
        RuleFor(x => x.DeskproTicketId).NotNull().GreaterThan(0);
        RuleFor(x => x.CaseTitle).NotEmpty();
    }
}
