using AktBob.Shared.Types.Podio;

namespace Aktbob.Modules.Podio.Features.UpdateTextField;
public interface IUpdateTextFieldHandler
{
    Task<ErrorOr<Success>> Handle(ItemId itemId, int fieldId, string textValue, CancellationToken cancellationToken);
}