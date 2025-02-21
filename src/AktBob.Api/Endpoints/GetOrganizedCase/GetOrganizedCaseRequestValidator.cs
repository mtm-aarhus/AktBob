using FastEndpoints;
using FluentValidation;

namespace AktBob.Api.Endpoints.GetOrganizedCase;
internal class GetOrganizedCaseRequestValidator : Validator<GetOrganizedCaseRequest>
{
    public GetOrganizedCaseRequestValidator()
    {
        RuleFor(x => x.DeskproTicketId).NotNull().GreaterThan(0);
        RuleFor(x => x.CaseTitle).NotEmpty();
    }
}
