using FastEndpoints;
using FluentValidation;

namespace AktBob.Podio;
internal class UpdatePodioFieldRequestValidator : Validator<UpdatePodioFieldRequest>
{
    public UpdatePodioFieldRequestValidator()
    {
        RuleFor(x => x.ItemId).NotNull();
        RuleFor(x => x.Value).NotEmpty();
    }
}
