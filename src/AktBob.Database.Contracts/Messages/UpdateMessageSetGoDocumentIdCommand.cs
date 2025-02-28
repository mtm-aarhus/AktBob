using AktBob.Database.Contracts.Dtos;

namespace AktBob.Database.Contracts.Messages;
public record UpdateMessageSetGoDocumentIdCommand(int DeskproMessageId, int? GoDocumentId) : IRequest<Result<MessageDto>>;