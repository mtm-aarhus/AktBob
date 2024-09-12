using MediatR;

namespace AktBob.DatabaseAPI.Contracts.Commands;
public record UpdateMessageSetJournalizedCommand(int Id, DateTime JournalizedAt, int GoDocumentId) : IRequest;