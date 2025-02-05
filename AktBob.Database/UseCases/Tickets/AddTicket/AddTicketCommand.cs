using AktBob.Database.Entities;
using Ardalis.Result;
using MassTransit.Mediator;

namespace AktBob.Database.UseCases.Tickets.AddTicket;
internal record AddTicketCommand(int DeskproTicketId) : Request<Result<Ticket>>;
