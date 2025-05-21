using FastEndpoints;
using FluentValidation;

namespace AktBob.Api.Endpoints.GetOrganizedCase;
internal class CreateGetOrganizedCaseRequestValidator : Validator<CreateGetOrganizedCaseRequest>
{
    public CreateGetOrganizedCaseRequestValidator()
    {
        RuleFor(x => x.DeskproTicketId).NotNull().GreaterThan(0);
    }
}
