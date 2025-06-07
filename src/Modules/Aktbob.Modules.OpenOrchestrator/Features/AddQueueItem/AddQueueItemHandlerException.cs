using System.Text.Json;
using ErrorOr;

namespace Aktbob.Modules.OpenOrchestrator.Features.AddQueueItem;

internal class AddQueueItemHandlerException(IAddQueueItemHandler next, ILogger<AddQueueItemHandler> logger)
    : IAddQueueItemHandler
{
    private readonly IAddQueueItemHandler _next = next;
    private readonly ILogger<AddQueueItemHandler> _logger = logger;

    public async Task<ErrorOr<Guid>> Handle(string queueName, string reference, JsonDocument? payload, CancellationToken cancellationToken)
    {
        try
        {
            return await _next.Handle(queueName, reference, payload, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(AddQueueItemHandler));
            return Error.Failure("AddQueueItemHandler.Failure", ex.Message);
        }
    }
}