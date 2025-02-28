using AktBob.Shared.CQRS;
using Ardalis.Result;

namespace AktBob.OpenOrchestrator.Contracts;
public record CreateQueueItemCommand(string QueueName, string Payload, string Reference) : ICommand<Result<Guid>>;
