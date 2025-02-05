using AktBob.Database.Entities;
using Ardalis.Result;
using MediatR;

namespace AktBob.Database.UseCases.Tickets.GetTicketById;
internal record GetTicketByIdQuery(int Id) : IRequest<Result<Ticket>>;