using System.Text.Json;
using AktBob.Shared.Contracts.Modules.OpenOrchestrator;
using AktBob.Shared.Extensions;
using ErrorOr;

namespace Aktbob.Modules.OpenOrchestrator.Features.CreateQueueItem;

internal class CreateQueueItemHandlerLogging(ICreateQueueItemHandler next, ILogger<CreateQueueItemHandler> logger) : ICreateQueueItemHandler
{
    private readonly ICreateQueueItemHandler _next = next;
    private readonly ILogger<CreateQueueItemHandler> _logger = logger;

    public async Task<ErrorOr<CreateQueueItemResponse>> Handle(string queueName, string reference, JsonDocument? payload, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Adding queue item. Queue = {queueName}, Reference = {reference}", queueName, reference);
        
        var result = await _next.Handle(queueName, reference, payload, cancellationToken);
        result.Switch(
            value => _logger.LogInformation("Queue item added, id = {id}", value),
            errors => _logger.LogError("Error adding queue item: {errors}", errors.ToCommaDelimitedString()));
        
        return result;
    }
}