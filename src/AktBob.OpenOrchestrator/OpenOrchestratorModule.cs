using AktBob.OpenOrchestrator.Contracts;
using AktBob.Shared;
using System.Text;

namespace AktBob.OpenOrchestrator;

internal class OpenOrchestratorModule(IJobDispatcher jobDispatcher) : IOpenOrchestratorModule
{
    public void CreateQueueItem(CreateQueueItemCommand command)
    {
        var bytes = Encoding.UTF8.GetBytes(command.Payload);
        var base64Payload = Convert.ToBase64String(bytes);
        jobDispatcher.Dispatch(new CreateQueueItemJob(command.QueueName, command.Reference, base64Payload));
    }
}
