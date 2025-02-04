using Ardalis.Result;
using MassTransit.Mediator;

namespace AktBob.OpenOrchestrator.Contracts;
public record CreateQueueItemCommand(string QueueName, object Data, string Reference) : Request<Result<Guid>>;
