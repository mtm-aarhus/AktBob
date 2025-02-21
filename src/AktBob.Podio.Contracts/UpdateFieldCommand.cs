using Ardalis.Result;
using MassTransit.Mediator;

namespace AktBob.Podio.Contracts;
public record UpdateFieldCommand(int AppId, long ItemId, int FieldId, string Value) : Request<Result>;