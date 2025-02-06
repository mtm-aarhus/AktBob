using FastEndpoints;
using FluentValidation;

namespace AktBob.ExternalQueue.Endpoints;
internal class AddCaseRequestValidator : Validator<AddCaseRequest>
{
    public AddCaseRequestValidator()
    {
        RuleFor(x => x.FilArkivCaseId).NotEmpty();
        RuleFor(x => x.PodioItemId).NotEmpty();
    }
}
