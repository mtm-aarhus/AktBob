namespace AktBob.OpenOrchestrator.Contracts;

public interface IOpenOrchestratorModule
{
    void CreateQueueItem(string QueueName, string Reference, string Payload);
}
