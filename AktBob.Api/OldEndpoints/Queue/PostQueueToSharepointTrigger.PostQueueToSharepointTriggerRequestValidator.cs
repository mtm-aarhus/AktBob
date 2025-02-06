using FastEndpoints;
using FluentValidation;

namespace AktBob.ExternalQueue.Endpoints;
internal class PostQueueToSharepointTriggerRequestValidator : Validator<PostQueueToSharepointTriggerRequest>
{
    public PostQueueToSharepointTriggerRequestValidator()
    {
        RuleFor(x => x.PodioItemId).NotNull();
    }
}
