using AktBob.Database.Entities;
using Ardalis.Result;
using MassTransit.Mediator;

namespace AktBob.Database.UseCases.Messages.GetMessageById;
internal record GetMessageByIdQuery(int Id) : Request<Result<Message>>;