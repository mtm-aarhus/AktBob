using AktBob.Database.Contracts.Dtos;

namespace AktBob.Database.Contracts.Messages;
public record GetMessageByDeskproMessageIdQuery(int DeskproMessageId) : IRequest<Result<MessageDto>>;
