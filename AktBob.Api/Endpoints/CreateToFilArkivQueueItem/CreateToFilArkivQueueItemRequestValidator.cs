using FastEndpoints;
using FluentValidation;

namespace AktBob.Api.Endpoints.CreateGoToFilArkivQueueItem;

internal class CreateToFilArkivQueueItemRequestValidator : Validator<CreateToFilArkivQueueItemRequest>
{
    public CreateToFilArkivQueueItemRequestValidator()
    {
        RuleFor(x => x.PodioItemId).NotNull();
    }
}
