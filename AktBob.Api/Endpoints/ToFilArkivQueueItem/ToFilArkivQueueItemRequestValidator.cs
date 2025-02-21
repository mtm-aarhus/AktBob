using FastEndpoints;
using FluentValidation;

namespace AktBob.Api.Endpoints.CreateGoToFilArkivQueueItem;

internal class ToFilArkivQueueItemRequestValidator : Validator<ToFilArkivQueueItemRequest>
{
    public ToFilArkivQueueItemRequestValidator()
    {
        RuleFor(x => x.PodioItemId).NotNull();
    }
}
