using AktBob.Shared.Contracts.Modules.OpenOrchestrator;
using ErrorOr;

namespace AktBob.Shared.ModuleClients.OpenOrchestratorModule;

public interface IOpenOrchestratorModuleClient
{
    Task<ErrorOr<CreateQueueItemResponse>> AddQueueItem(string queueName, string reference, object? payload, CancellationToken cancellationToken = default);
}