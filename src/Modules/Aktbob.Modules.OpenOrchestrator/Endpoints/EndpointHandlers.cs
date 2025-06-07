using Aktbob.Modules.OpenOrchestrator.Contracts;
using Aktbob.Modules.OpenOrchestrator.Features.AddQueueItem;
using AktBob.Shared.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Aktbob.Modules.OpenOrchestrator.Endpoints;

internal static class EndpointHandlers
{
    public static async Task<IResult> AddQueueItem([FromBody] AddQueueItemRequest request, [FromServices] IAddQueueItemHandler handler, CancellationToken cancellationToken)
    {
        var result = await handler.Handle(request.QueueName, request.Reference, request.Payload, cancellationToken);
        return result.ToMinimalApiResponse();
    }
}