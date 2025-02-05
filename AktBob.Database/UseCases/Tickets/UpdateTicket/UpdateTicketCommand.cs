using AktBob.Database.Entities;
using Ardalis.Result;
using MassTransit.Mediator;

namespace AktBob.Database.UseCases.Tickets.UpdateTicket;
internal record UpdateTicketCommand(int Id, string? CaseNumber, string? SharepointFolderName, DateTime? TicketClosedAt, DateTime? JournalizedAt) : Request<Result<Ticket>>;