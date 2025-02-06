using FastEndpoints;
using FluentValidation;

namespace AktBob.ExternalQueue.Endpoints;
internal class PostQueueDocumentListTriggerRequestValidator : Validator<PostQueueDocumentListTriggerRequest>
{
    public PostQueueDocumentListTriggerRequestValidator()
    {
        RuleFor(x => x.PodioItemId).NotNull();
    }
}
