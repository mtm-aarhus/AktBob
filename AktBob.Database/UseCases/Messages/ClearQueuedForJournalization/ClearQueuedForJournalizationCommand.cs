using Ardalis.Result;
using MassTransit.Mediator;

namespace AktBob.Database.UseCases.Messages.ClearQueuedForJournalization;
internal record ClearQueuedForJournalizationCommand(int Id) : Request<Result>;