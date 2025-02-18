using AktBob.Database.Contracts.Dtos;
using Ardalis.Result;
using MassTransit.Mediator;

namespace AktBob.Database.Contracts.Messages;
public record UpdateMessageSetGoDocumentIdCommand(int DeskproMessageId, int? GoDocumentId) : Request<Result<MessageDto>>;