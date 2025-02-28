namespace AktBob.Database.Contracts.Messages;
public record AddMessageCommand(int TicketId, int DeskproMessageId) : IRequest<Result<int>>;
