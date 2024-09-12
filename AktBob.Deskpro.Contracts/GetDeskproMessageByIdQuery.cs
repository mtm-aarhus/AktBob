using AAK.Deskpro.Models;
using Ardalis.Result;
using MediatR;

namespace AktBob.Deskpro.Contracts;
public record GetDeskproMessageByIdQuery(int TicketId, int MessageId) : IRequest<Result<Message>>;
