using AktBob.Database.Entities;
using Ardalis.Result;
using MediatR;

namespace AktBob.Database.UseCases.Messages.GetMessageByDeskproMessageId;
internal record GetMessageByDeskproMessageIdQuery(int DeskproMessageId) : IRequest<Result<IEnumerable<Message>>>;
