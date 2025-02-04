using Ardalis.Result;
using MassTransit.Mediator;

namespace AktBob.Queue.Contracts;
public record GetQueueMessagesQuery(string QueueName, int MaxMessages = 10, int VisibilityvisibilyTimeoutSeconds = 60) : Request<Result<IEnumerable<QueueMessageDto>>>;