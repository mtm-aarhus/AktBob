using FastEndpoints;
using FluentValidation;

namespace AktBob.Api.Endpoints.CreateCleanUpQueueItems;

internal class CreateCleanUpQueueItemsRequestValidator : Validator<CreateCleanUpQueueItemsRequest>
{
    public CreateCleanUpQueueItemsRequestValidator()
    {
        RuleFor(x => x.DeskproTicketId.Value).NotNull().GreaterThan(0);
    }
}
