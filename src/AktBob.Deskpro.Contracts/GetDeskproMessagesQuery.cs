using AktBob.Deskpro.Contracts.DTOs;

namespace AktBob.Deskpro.Contracts;
public record GetDeskproMessagesQuery(int TicketId) : IRequest<Result<IEnumerable<MessageDto>>>;