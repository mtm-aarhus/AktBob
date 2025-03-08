using AktBob.Shared;

namespace AktBob.Podio.Contracts;

public record UpdateTextFieldCommand(PodioItemId PodioItemId, int FieldId, string TextValue);