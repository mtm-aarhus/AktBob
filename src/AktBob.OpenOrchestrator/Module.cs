using AktBob.OpenOrchestrator.Contracts;
using AktBob.Shared;

namespace AktBob.OpenOrchestrator;

internal class Module(IJobDispatcher jobDispatcher) : IOpenOrchestratorModule
{
    public void CreateQueueItem(string queueName, string reference, string payload) => jobDispatcher.Dispatch(new CreateQueueItemJob(queueName, reference, payload));
}
