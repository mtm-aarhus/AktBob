using MediatR;

namespace AktBob.Queue.Contracts;
public record DeleteQueueMessageCommand(string ConnectionString, string QueueName, string MessageId, string PopReciept) : IRequest;