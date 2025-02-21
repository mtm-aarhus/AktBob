using FastEndpoints;
using FluentValidation;

namespace AktBob.Api.Endpoints.DocumentListQueueItem;

internal class DocumentListQueueItemRequestValidator : Validator<DocumentListQueueItemRequest>
{
    public DocumentListQueueItemRequestValidator()
    {
        RuleFor(x => x.PodioItemId).NotNull();
    }
}
