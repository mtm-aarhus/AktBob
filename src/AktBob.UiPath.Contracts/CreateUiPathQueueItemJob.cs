namespace AktBob.UiPath.Contracts;
public record CreateUiPathQueueItemJob(string QueueName, string Reference, string Payload);