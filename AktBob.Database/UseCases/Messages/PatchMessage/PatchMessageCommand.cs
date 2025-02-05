using AktBob.Database.Entities;
using Ardalis.Result;
using MassTransit.Mediator;

namespace AktBob.Database.UseCases.Messages.PatchMessage;
internal record PatchMessageCommand(int Id, int? GoDocumentId) : Request<Result<Message>>;