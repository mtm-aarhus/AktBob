using AktBob.Database.Contracts.Dtos;
using Ardalis.Result;
using MassTransit.Mediator;

namespace AktBob.Database.UseCases.Tickets.UpdateTicket;
public record UpdateTicketCommand(int Id, string? CaseNumber, string? SharepointFolderName, DateTime? TicketClosedAt, DateTime? JournalizedAt) : Request<Result<TicketDto>>;