using AktBob.OpenOrchestrator.Contracts;
using AktBob.Shared;

namespace AktBob.OpenOrchestrator;

internal class OpenOrchestratorModule(IJobDispatcher jobDispatcher) : IOpenOrchestratorModule
{
    public void CreateQueueItem(CreateQueueItemCommand command) => jobDispatcher.Dispatch(new CreateQueueItemJob(command.QueueName, command.Reference, command.Payload));
}
