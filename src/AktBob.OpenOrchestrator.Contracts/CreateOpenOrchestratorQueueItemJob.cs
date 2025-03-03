namespace AktBob.OpenOrchestrator.Contracts;
public record CreateOpenOrchestratorQueueItemJob(string QueueName, string Reference, string Payload);
