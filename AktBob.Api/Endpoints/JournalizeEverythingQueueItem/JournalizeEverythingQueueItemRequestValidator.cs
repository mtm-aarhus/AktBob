using FastEndpoints;
using FluentValidation;

namespace AktBob.Api.Endpoints.JournalizeEverythingQueueItem;

internal class JournalizeEverythingQueueItemRequestValidator : Validator<JournalizeEverythingQueueItemRequest>
{
    public JournalizeEverythingQueueItemRequestValidator()
    {
        RuleFor(x => x.DeskproId).NotNull();
    }
}
