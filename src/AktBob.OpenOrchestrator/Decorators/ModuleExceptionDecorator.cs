using AktBob.OpenOrchestrator.Contracts;
using Microsoft.Extensions.Logging;

namespace AktBob.OpenOrchestrator.Decorators;

internal class ModuleExceptionDecorator : IOpenOrchestratorModule
{
    private readonly IOpenOrchestratorModule _inner;
    private readonly ILogger<OpenOrchestratorModule> _logger;

    public ModuleExceptionDecorator(IOpenOrchestratorModule inner, ILogger<OpenOrchestratorModule> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public void CreateQueueItem(CreateQueueItemCommand command)
    {
        try
        {
            _inner.CreateQueueItem(command);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enqueing job: {name}", nameof(CreateQueueItem));
            throw;
        }
    }
}
