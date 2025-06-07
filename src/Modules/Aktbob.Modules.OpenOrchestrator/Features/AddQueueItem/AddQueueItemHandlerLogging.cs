using System.Text.Json;
using AktBob.Shared.Extensions;
using ErrorOr;

namespace Aktbob.Modules.OpenOrchestrator.Features.AddQueueItem;

internal class AddQueueItemHandlerLogging(IAddQueueItemHandler next, ILogger<AddQueueItemHandler> logger) : IAddQueueItemHandler
{
    private readonly IAddQueueItemHandler _next = next;
    private readonly ILogger<AddQueueItemHandler> _logger = logger;

    public async Task<ErrorOr<Guid>> Handle(string queueName, string reference, JsonDocument? payload, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Adding queue item. Queue = {queueName}, Reference = {reference}", queueName, reference);
        
        var result = await _next.Handle(queueName, reference, payload, cancellationToken);
        result.Switch(
            value => _logger.LogInformation("Queue item added, id = {id}", value),
            errors => _logger.LogError("Error adding queue item: {errors}", errors.ToCommaDelimitedString()));
        
        return result;
    }
}