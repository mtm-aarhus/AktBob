using AktBob.Database.Contracts.Dtos;
using Ardalis.Result;
using MassTransit.Mediator;

namespace AktBob.Database.Contracts;
public record UpdateMessageCommand(int Id, int? GoDocumentId) : Request<Result<MessageDto>>;