using AktBob.Shared.Types.Podio;

namespace AktBob.Podio.Contracts;

public record UpdateTextFieldCommand(ItemId PodioItemId, int FieldId, string TextValue);