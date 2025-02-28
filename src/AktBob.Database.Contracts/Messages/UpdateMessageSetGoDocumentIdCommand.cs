namespace AktBob.Database.Contracts.Messages;
public record UpdateMessageSetGoDocumentIdCommand(int DeskproMessageId, int? GoDocumentId) : ICommand<Result<MessageDto>>;