namespace AktBob.Database.Contracts.Messages;
public record GetMessageByDeskproMessageIdQuery(int DeskproMessageId) : IQuery<Result<MessageDto>>;
