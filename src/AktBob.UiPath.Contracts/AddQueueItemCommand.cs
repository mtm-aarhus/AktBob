using MediatR;

namespace AktBob.UiPath.Contracts;

public record AddQueueItemCommand(string QueueName, string Reference, string Payload) : IRequest;