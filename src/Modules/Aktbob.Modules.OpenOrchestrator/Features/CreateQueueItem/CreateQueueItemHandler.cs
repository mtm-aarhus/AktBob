using System.Text.Json;
using Aktbob.Modules.OpenOrchestrator.Client;
using AktBob.Shared.Contracts.Modules.OpenOrchestrator;
using ErrorOr;

namespace Aktbob.Modules.OpenOrchestrator.Features.CreateQueueItem;

internal class CreateQueueItemHandler(OpenOrchestratorClient openOrchestratorClient) : ICreateQueueItemHandler
{
    private readonly OpenOrchestratorClient _openOrchestratorClient = openOrchestratorClient;

    public async Task<ErrorOr<CreateQueueItemResponse>> Handle(string queueName, string reference, JsonDocument? payload, CancellationToken cancellationToken)
    {
        var result = await _openOrchestratorClient.PostQueueItem(queueName, reference, payload, cancellationToken);
        if (result == null)
        {
            return Error.Failure("OpenOrchestrator.AddQueueItemHandler", "Error posting queue item");
        }

        return result;
    }
}