using AktBob.Deskpro.Contracts.DTOs;
using MediatR;

namespace AktBob.Deskpro.Contracts;
public record GetDeskproMessagesQuery(int TicketId) : IRequest<IEnumerable<MessageDto>>;