using AktBob.Database.Entities;
using Ardalis.Result;
using MassTransit.Mediator;

namespace AktBob.Database.UseCases.Messages.GetMessageByDeskproMessageId;
internal record GetMessageByDeskproMessageIdQuery(int DeskproMessageId) : Request<Result<IEnumerable<Message>>>;
