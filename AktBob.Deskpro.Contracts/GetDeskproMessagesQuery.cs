using AktBob.Deskpro.Contracts.DTOs;
using MassTransit.Mediator;

namespace AktBob.Deskpro.Contracts;
public record GetDeskproMessagesQuery(int TicketId) : Request<IEnumerable<MessageDto>>;