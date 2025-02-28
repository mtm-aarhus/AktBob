using Ardalis.Result;
using MediatR;

namespace AktBob.OpenOrchestrator.Contracts;
public record CreateQueueItemCommand(string QueueName, string Payload, string Reference) : IRequest<Result<Guid>>;
