using FastEndpoints;
using FluentValidation;

namespace AktBob.Api.Endpoints.CheckOCRScreeningStatus;
internal class CheckOCRScreeningRequestValidator : Validator<CheckOCRScreeningRequest>
{
    public CheckOCRScreeningRequestValidator()
    {
        RuleFor(x => x.FilArkivCaseId).NotEmpty();
        RuleFor(x => x.PodioItemId).NotEmpty();
    }
}
