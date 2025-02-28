using MediatR;

namespace AktBob.Podio.Contracts;
public record UpdateFieldCommand(int AppId, long ItemId, int FieldId, string Value) : IRequest;