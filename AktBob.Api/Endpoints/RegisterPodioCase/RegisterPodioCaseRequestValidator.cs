using FastEndpoints;
using FluentValidation;

namespace AktBob.Api.Endpoints.RegisterPodioCase;

internal class RegisterPodioCaseRequestValidator : Validator<RegisterPodioCaseRequet>
{
    public RegisterPodioCaseRequestValidator()
    {
        RuleFor(x => x.PodioItemId).NotNull();
    }
}
