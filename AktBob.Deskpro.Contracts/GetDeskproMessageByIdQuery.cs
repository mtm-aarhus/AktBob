using AktBob.Deskpro.Contracts.DTOs;
using Ardalis.Result;
using MassTransit.Mediator;

namespace AktBob.Deskpro.Contracts;
public record GetDeskproMessageByIdQuery(int TicketId, int MessageId) : Request<Result<MessageDto>>;
