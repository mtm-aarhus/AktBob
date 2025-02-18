using Ardalis.Result;
using MassTransit.Mediator;

namespace AktBob.Database.Contracts.Messages;
public record AddMessageCommand(int TicketId, int DeskproMessageId) : Request<Result<int>>;
