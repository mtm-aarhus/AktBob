using Aktbob.Modules.OpenOrchestrator.Features.CreateQueueItem;
using AktBob.Shared.Contracts.Modules.OpenOrchestrator;
using AktBob.Shared.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Aktbob.Modules.OpenOrchestrator.Endpoints;

internal static class EndpointHandlers
{
    public static async Task<IResult> AddQueueItem([FromBody] CreateQueueItemRequest request, [FromServices] ICreateQueueItemHandler handler, CancellationToken cancellationToken)
    {
        var result = await handler.Handle(request.QueueName, request.Reference, request.Payload, cancellationToken);
        return result.ToMinimalApiResponse(value => value);
    }
}