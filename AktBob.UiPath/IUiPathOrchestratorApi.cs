
namespace AktBob.UiPath;

public interface IUiPathOrchestratorApi
{
    Task AddQueueItem(string queueName, string reference, object queueItem);
}