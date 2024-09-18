using AktBob.Deskpro.Contracts.DTOs;
using Ardalis.Result;
using MediatR;

namespace AktBob.Deskpro.Contracts;
public record GetDeskproMessageByIdQuery(int TicketId, int MessageId) : IRequest<Result<MessageDto>>;
