using AktBob.Database.Contracts.Dtos;
using Ardalis.Result;
using MassTransit.Mediator;

namespace AktBob.Database.Contracts;
public record GetMessageByDeskproMessageIdQuery(int DeskproMessageId) : Request<Result<MessageDto>>;
