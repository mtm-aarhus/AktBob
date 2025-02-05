using AktBob.Database.Entities;
using Ardalis.Result;
using MassTransit.Mediator;

namespace AktBob.Database.UseCases.Tickets.PatchTicket;
internal record PatchTicketCommand(int Id, string? CaseNumber, string? SharepointFolderName, DateTime? TicketClosedAt, DateTime? JournalizedAt) : Request<Result<Ticket>>;