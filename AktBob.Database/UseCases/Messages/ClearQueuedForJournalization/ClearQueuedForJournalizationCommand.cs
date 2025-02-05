using Ardalis.Result;
using MediatR;

namespace AktBob.Database.UseCases.Messages.ClearQueuedForJournalization;
internal record ClearQueuedForJournalizationCommand(int Id) : IRequest<Result>;