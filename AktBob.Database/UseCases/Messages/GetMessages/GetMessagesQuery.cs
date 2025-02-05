using AktBob.Database.Entities;
using Ardalis.Result;
using MassTransit.Mediator;

namespace AktBob.Database.UseCases.Messages.GetMessages;
internal record GetMessagesQuery(bool IncludeJournalized) : Request<Result<IEnumerable<Message>>>;