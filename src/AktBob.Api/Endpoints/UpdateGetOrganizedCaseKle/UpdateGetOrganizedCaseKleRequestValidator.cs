using FastEndpoints;
using FluentValidation;

namespace AktBob.Api.Endpoints.UpdateGetOrganizedCaseKle;

internal class UpdateGetOrganizedCaseKleRequestValidator : Validator<UpdateGetOrganizedCaseKleRequest>
{
    public UpdateGetOrganizedCaseKleRequestValidator()
    {
        RuleFor(x => x.TargetCaseId).NotEmpty();
        RuleFor(x => x.SourceCaseId).NotEmpty();
    }
}
