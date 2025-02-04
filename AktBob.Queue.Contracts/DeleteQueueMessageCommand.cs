namespace AktBob.Queue.Contracts;
public record DeleteQueueMessageCommand(string QueueName, string MessageId, string PopReciept);