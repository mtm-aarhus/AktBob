using FastEndpoints;
using FluentValidation;

namespace AktBob.Api.Endpoints.AddOrUpdateDeskproTicketToGetOrganized;

internal class AddOrUpdateDeskproTicketToGetOrganizedRequestValidator : Validator<AddOrUpdateDeskproTicketToGetOrganizedRequest>
{
    public AddOrUpdateDeskproTicketToGetOrganizedRequestValidator()
    {
        RuleFor(x => x.TicketId).GreaterThan(0);
        RuleFor(x => x.GOCaseNumber).NotEmpty();
        RuleFor(x => x.CustomFieldIds).NotNull();
        RuleFor(x => x.CaseNumberFieldIds).NotNull();
    }
}
