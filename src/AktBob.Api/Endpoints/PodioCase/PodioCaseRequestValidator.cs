using FastEndpoints;
using FluentValidation;

namespace AktBob.Api.Endpoints.PodioCase;

internal class PodioCaseRequestValidator : Validator<PodioCaseRequet>
{
    public PodioCaseRequestValidator()
    {
        RuleFor(x => x.PodioItemId).NotNull();
    }
}
