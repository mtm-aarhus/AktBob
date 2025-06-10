using AktBob.Shared.Contracts.Modules.OpenOrchestrator;
using AktBob.Shared.Extensions;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace AktBob.Shared.ModuleClients.OpenOrchestratorModule;

internal class OpenOrchestratorModuleClientLogging(IOpenOrchestratorModuleClient next, ILogger<OpenOrchestratorModuleClient> logger) : IOpenOrchestratorModuleClient
{
    public async Task<ErrorOr<CreateQueueItemResponse>> AddQueueItem(string queueName, string reference, object? payload, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating OpenOrchestrator queue item (queue={queue})", queueName);
        
        var result = await next.AddQueueItem(queueName, reference, payload, cancellationToken);
        result.Switch(
            value => logger.LogInformation("OpenOrchestrator queue item created successfully (queue={queue}, id={id})", queueName, value.Id),
            _ => result.LogResultErrors(logger));
        
        return result;
        ;
    }
}