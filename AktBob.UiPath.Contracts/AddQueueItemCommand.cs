namespace AktBob.UiPath.Contracts;

public record AddQueueItemCommand(string QueueName, string Reference, object QueueItem);