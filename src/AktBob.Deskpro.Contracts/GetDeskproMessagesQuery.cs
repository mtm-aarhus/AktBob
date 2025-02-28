using AktBob.Deskpro.Contracts.DTOs;

namespace AktBob.Deskpro.Contracts;
public record GetDeskproMessagesQuery(int TicketId) : IQuery<Result<IEnumerable<MessageDto>>>;