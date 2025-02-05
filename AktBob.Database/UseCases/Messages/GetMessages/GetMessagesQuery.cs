using AktBob.Database.Entities;
using Ardalis.Result;
using MediatR;

namespace AktBob.Database.UseCases.Messages.GetMessages;
internal record GetMessagesQuery(bool IncludeJournalized) : IRequest<Result<IEnumerable<Message>>>;