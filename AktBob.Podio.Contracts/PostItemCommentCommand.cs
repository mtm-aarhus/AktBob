using Ardalis.Result;
using MassTransit.Mediator;

namespace AktBob.Podio.Contracts;
public record PostItemCommentCommand(int AppId, long ItemId, string Comment) : Request<Result>;