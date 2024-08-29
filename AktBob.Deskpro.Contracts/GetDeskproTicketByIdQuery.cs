using AAK.Deskpro.Models;
using Ardalis.Result;
using MediatR;

namespace AktBob.Deskpro.Contracts;
public record GetDeskproTicketByIdQuery(int Id) : IRequest<Result<Ticket>>;