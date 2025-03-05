using FastEndpoints;
using FluentValidation;

namespace AktBob.Api.Endpoints.UpdateDeskproSetGetOrganizedAggregatedCases;

internal class UpdateDeskproSetGetOrganizedAggregatedCaseNumbersRequestValidator : Validator<UpdateDeskproSetGetOrganizedAggregatedCaseNumbersRequest>
{
    public UpdateDeskproSetGetOrganizedAggregatedCaseNumbersRequestValidator()
    {
        RuleFor(x => x.CaseIds).NotEmpty();
        RuleFor(x => x.DeskproTicketId).NotNull().GreaterThan(0);
    }
}
