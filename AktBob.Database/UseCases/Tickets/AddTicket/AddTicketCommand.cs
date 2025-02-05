using AktBob.Database.Entities;
using Ardalis.Result;
using MediatR;

namespace AktBob.Database.UseCases.Tickets.AddTicket;
internal record AddTicketCommand(int DeskproTicketId) : IRequest<Result<Ticket>>;
