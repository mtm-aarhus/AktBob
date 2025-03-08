using AktBob.OpenOrchestrator.Contracts;
using Microsoft.Extensions.Logging;

namespace AktBob.OpenOrchestrator;

internal class ModuleLoggingDecorator : IOpenOrchestratorModule
{
    private readonly IOpenOrchestratorModule _inner;
    private readonly ILogger<ModuleLoggingDecorator> _logger;

    public ModuleLoggingDecorator(IOpenOrchestratorModule inner, ILogger<ModuleLoggingDecorator> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public void CreateQueueItem(CreateQueueItemCommand command)
    {
        _logger.LogInformation("Enqueuing job: Create OpenOrchestrator queue item. {command}", command);
        _inner.CreateQueueItem(command);
    }
}
