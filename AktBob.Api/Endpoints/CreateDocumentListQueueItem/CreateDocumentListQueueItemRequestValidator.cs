using FastEndpoints;
using FluentValidation;

namespace AktBob.Api.Endpoints.CreateDocumentListQueueItem;

internal class CreateDocumentListQueueItemRequestValidator : Validator<CreateDocumentListQueueItemRequest>
{
    public CreateDocumentListQueueItemRequestValidator()
    {
        RuleFor(x => x.PodioItemId).NotNull();
    }
}
