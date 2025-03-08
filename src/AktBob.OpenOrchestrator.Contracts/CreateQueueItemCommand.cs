namespace AktBob.OpenOrchestrator.Contracts;

public record CreateQueueItemCommand(string QueueName, string Reference, string Payload);