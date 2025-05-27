using AktBob.Shared.Types.Podio;

namespace AktBob.Podio.Handlers.UpdateTextField;
internal interface IUpdateTextFieldHandler
{
    Task<ErrorOr<Success>> Handle(ItemId itemId, int fieldId, string textValue, CancellationToken cancellationToken);
}