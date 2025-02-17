using FastEndpoints;
using FluentValidation;

namespace AktBob.Api.Endpoints.CreateAfgørelsesskrivelseQueueItem;

public class CreateAfgørelsesskrivelseQueueItemRequestValidator : Validator<CreateAfgørelsesskrivelseQueueItemRequest>
{
    public CreateAfgørelsesskrivelseQueueItemRequestValidator()
    {
        RuleFor(x => x.DeskproId).NotNull();
        RuleFor(x => x.DeskproId).GreaterThan(0);
    }
}
