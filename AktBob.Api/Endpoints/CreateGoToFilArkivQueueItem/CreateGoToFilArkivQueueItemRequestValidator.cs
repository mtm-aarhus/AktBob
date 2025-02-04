using FastEndpoints;
using FluentValidation;

namespace AktBob.Api.Endpoints.CreateGoToFilArkivQueueItem;

internal class CreateGoToFilArkivQueueItemRequestValidator : Validator<CreateGoToFilArkivQueueItemRequest>
{
    public CreateGoToFilArkivQueueItemRequestValidator()
    {
        RuleFor(x => x.PodioItemId).NotNull();
    }
}
