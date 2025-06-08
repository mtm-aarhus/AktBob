using System.Text.Json;
using AktBob.Shared.Contracts.Modules.OpenOrchestrator;
using ErrorOr;

namespace Aktbob.Modules.OpenOrchestrator.Features.CreateQueueItem;

internal class CreateQueueItemHandlerException(ICreateQueueItemHandler next, ILogger<CreateQueueItemHandler> logger)
    : ICreateQueueItemHandler
{
    private readonly ICreateQueueItemHandler _next = next;
    private readonly ILogger<CreateQueueItemHandler> _logger = logger;

    public async Task<ErrorOr<CreateQueueItemResponse>> Handle(string queueName, string reference, JsonDocument? payload, CancellationToken cancellationToken)
    {
        try
        {
            return await _next.Handle(queueName, reference, payload, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(CreateQueueItemHandler));
            return Error.Failure("AddQueueItemHandler.Failure", ex.Message);
        }
    }
}