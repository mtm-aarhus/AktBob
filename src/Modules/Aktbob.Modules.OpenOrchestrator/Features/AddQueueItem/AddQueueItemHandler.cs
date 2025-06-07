using System.Text.Json;
using Aktbob.Modules.OpenOrchestrator.Client;
using ErrorOr;

namespace Aktbob.Modules.OpenOrchestrator.Features.AddQueueItem;

internal class AddQueueItemHandler(OpenOrchestratorClient openOrchestratorClient) : IAddQueueItemHandler
{
    private readonly OpenOrchestratorClient _openOrchestratorClient = openOrchestratorClient;

    public async Task<ErrorOr<Guid>> Handle(string queueName, string reference, JsonDocument? payload, CancellationToken cancellationToken)
    {
        var result = await _openOrchestratorClient.PostQueueItem(queueName, reference, payload, cancellationToken);
        if (result == null)
        {
            return Error.Failure("OpenOrchestrator.AddQueueItemHandler", "Error posting queue item");
        }

        return result.Id;
    }
}