using AktBob.Database.Entities;
using Ardalis.Result;
using MediatR;

namespace AktBob.Database.UseCases.Messages.GetMessageById;
internal record GetMessageByIdQuery(int Id) : IRequest<Result<Message>>;