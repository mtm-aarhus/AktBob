using FastEndpoints;
using FluentValidation;

namespace AktBob.ExternalQueue.Endpoints;
internal class CreateAktindsigtssagAddQueueMessageValidator : Validator<CreateAktindsigtssagAddQueueMessageRequest>
{
    public CreateAktindsigtssagAddQueueMessageValidator()
    {
        RuleFor(x => x.DeskproTicketId).NotNull().GreaterThan(0);
        RuleFor(x => x.CaseTitle).NotEmpty();
    }
}
