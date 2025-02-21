using FastEndpoints;
using FluentValidation;

namespace AktBob.Api.Endpoints.DeskproTicketToGetOrganized;

internal class DeskproTicketToGetOrganizedRequestValidator : Validator<DeskproTicketToGetOrganizedRequest>
{
    public DeskproTicketToGetOrganizedRequestValidator()
    {
        RuleFor(x => x.TicketId).GreaterThan(0);
        RuleFor(x => x.GOCaseNumber).NotEmpty();
        RuleFor(x => x.CustomFieldIds).NotNull();
        RuleFor(x => x.CaseNumberFieldIds).NotNull();
    }
}
