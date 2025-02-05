using FastEndpoints;
using FluentValidation;

namespace AktBob.Api.Endpoints.CreateToSharepointQueueItem;

internal class CreateToSharepointQueueItemRequestValidator : Validator<CreateToSharepointQueueItemRequest>
{
    public CreateToSharepointQueueItemRequestValidator()
    {
        RuleFor(x => x.PodioItemId).NotNull();
    }
}
