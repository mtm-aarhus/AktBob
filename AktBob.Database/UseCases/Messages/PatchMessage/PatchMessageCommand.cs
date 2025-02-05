using AktBob.Database.Entities;
using Ardalis.Result;
using MediatR;

namespace AktBob.Database.UseCases.Messages.PatchMessage;
internal record PatchMessageCommand(int Id, int? GoDocumentId) : IRequest<Result<Message>>;