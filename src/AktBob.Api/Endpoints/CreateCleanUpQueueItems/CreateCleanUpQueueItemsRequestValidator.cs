using FastEndpoints;
using FluentValidation;

namespace AktBob.Api.Endpoints.CreateCleanUpQueueItems;

internal class CreateCleanUpQueueItemsRequestValidator : Validator<CreateCleanUpQueueItemsRequest>
{
    public CreateCleanUpQueueItemsRequestValidator()
    {
        RuleFor(x => x.DeskproTicketId).NotNull().GreaterThan(0);
    }
}
