using AktBob.Shared;
using AktBob.UiPath.Contracts;

namespace AktBob.UiPath;

internal class Module(IJobDispatcher jobDispatcher) : IUiPathModule
{
    public void CreateQueueItem(string queueName, string reference, string payload) => jobDispatcher.Dispatch(new CreateQueueItemJob(queueName, reference, payload));
}
