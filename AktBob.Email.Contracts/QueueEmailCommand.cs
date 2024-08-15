using MediatR;

namespace AktBob.Email.Contracts;
public record QueueEmailCommand(string To, string Subject, string Body) : IRequest;
