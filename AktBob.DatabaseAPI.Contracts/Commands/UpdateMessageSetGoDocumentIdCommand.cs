using MediatR;

namespace AktBob.DatabaseAPI.Contracts.Commands;
public record UpdateMessageSetGoDocumentIdCommand(int Id, int GoDocumentId) : IRequest;