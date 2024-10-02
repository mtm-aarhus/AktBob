using MediatR;

namespace AktBob.DatabaseAPI.Contracts.Commands;
public record DeleteMessageCommand(int Id) : IRequest;