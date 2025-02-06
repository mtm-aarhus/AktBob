using FastEndpoints;
using FluentValidation;

namespace AktBob.ExternalQueue.Endpoints;
internal class PostQueueJournalizeDeskproTicketValidator : Validator<PostQueueJournalizeDeskproTicketRequest>
{
    public PostQueueJournalizeDeskproTicketValidator()
    {
        RuleFor(x => x.TicketId).GreaterThan(0);
        RuleFor(x => x.GOCaseNumber).NotEmpty();
        RuleFor(x => x.CustomFieldIds).NotNull();
        RuleFor(x => x.CaseNumberFieldIds).NotNull();
    }
}
