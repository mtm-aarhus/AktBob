using Ardalis.Result;
using MediatR;

namespace AktBob.OpenOrchestrator.Contracts;
public record CreateQueueItemCommand(string QueueName, string Status, object Data, string Reference) : IRequest<Result<Guid>>;
