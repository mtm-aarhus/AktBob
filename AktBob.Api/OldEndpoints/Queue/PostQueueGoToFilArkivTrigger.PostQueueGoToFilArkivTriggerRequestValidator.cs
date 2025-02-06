using FastEndpoints;
using FluentValidation;

namespace AktBob.ExternalQueue.Endpoints;
internal class PostQueueGoToFilArkivTriggerRequestValidator : Validator<PostQueueGoToFilArkivTriggerRequest>
{
    public PostQueueGoToFilArkivTriggerRequestValidator()
    {
        RuleFor(x => x.PodioItemId).NotNull();
    }
}
