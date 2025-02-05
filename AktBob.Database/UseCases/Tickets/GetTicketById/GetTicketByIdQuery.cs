using AktBob.Database.Entities;
using Ardalis.Result;
using MassTransit.Mediator;

namespace AktBob.Database.UseCases.Tickets.GetTicketById;
internal record GetTicketByIdQuery(int Id) : Request<Result<Ticket>>;