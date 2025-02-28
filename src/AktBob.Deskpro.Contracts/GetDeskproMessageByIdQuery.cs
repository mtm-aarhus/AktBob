using AktBob.Deskpro.Contracts.DTOs;

namespace AktBob.Deskpro.Contracts;
public record GetDeskproMessageByIdQuery(int TicketId, int MessageId) : IQuery<Result<MessageDto>>;
