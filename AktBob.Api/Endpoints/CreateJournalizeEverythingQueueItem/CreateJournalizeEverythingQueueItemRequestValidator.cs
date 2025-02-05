using FastEndpoints;
using FluentValidation;

namespace AktBob.Api.Endpoints.CreateJournalizeEverythingQueueItem;

internal class CreateJournalizeEverythingQueueItemRequestValidator : Validator<CreateJournalizeEverythingQueueItemRequest>
{
    public CreateJournalizeEverythingQueueItemRequestValidator()
    {
        RuleFor(x => x.DeskproId).NotNull();
    }
}
