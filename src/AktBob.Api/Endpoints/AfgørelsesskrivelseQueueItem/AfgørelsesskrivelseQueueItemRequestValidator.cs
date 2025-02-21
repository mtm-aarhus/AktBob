using AktBob.Api.Endpoints.CreateAfgørelsesskrivelseQueueItem;
using FastEndpoints;
using FluentValidation;

namespace AktBob.Api.Endpoints.AfgørelsesskrivelseQueueItem;

public class AfgørelsesskrivelseQueueItemRequestValidator : Validator<AfgørelsesskrivelseQueueItemRequest>
{
    public AfgørelsesskrivelseQueueItemRequestValidator()
    {
        RuleFor(x => x.DeskproId).NotNull();
        RuleFor(x => x.DeskproId).GreaterThan(0);
    }
}
