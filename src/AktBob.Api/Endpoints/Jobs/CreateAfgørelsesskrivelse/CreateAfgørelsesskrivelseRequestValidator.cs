using FastEndpoints;
using FluentValidation;

namespace AktBob.Api.Endpoints.Jobs.CreateAfgørelsesskrivelse;

public class CreateAfgørelsesskrivelseRequestValidator : Validator<CreateAfgørelsesskrivelseRequest>
{
    public CreateAfgørelsesskrivelseRequestValidator()
    {
        RuleFor(x => x.DeskproId).NotNull();
        RuleFor(x => x.DeskproId).GreaterThan(0);
    }
}
