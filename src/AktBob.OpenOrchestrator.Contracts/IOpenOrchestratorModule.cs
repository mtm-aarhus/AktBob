namespace AktBob.OpenOrchestrator.Contracts;

public interface IOpenOrchestratorModule
{
    void CreateQueueItem(CreateQueueItemCommand command);
}
