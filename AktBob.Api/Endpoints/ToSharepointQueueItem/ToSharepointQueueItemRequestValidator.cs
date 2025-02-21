using FastEndpoints;
using FluentValidation;

namespace AktBob.Api.Endpoints.ToSharepointQueueItem;

internal class ToSharepointQueueItemRequestValidator : Validator<ToSharepointQueueItemRequest>
{
    public ToSharepointQueueItemRequestValidator()
    {
        RuleFor(x => x.PodioItemId).NotNull();
    }
}
