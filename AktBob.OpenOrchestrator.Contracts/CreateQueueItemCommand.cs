using Ardalis.Result;
using MassTransit.Mediator;

namespace AktBob.OpenOrchestrator.Contracts;
public record CreateQueueItemCommand(string QueueName, string Payload, string Reference) : Request<Result<Guid>>;
