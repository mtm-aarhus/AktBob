using AktBob.Shared.Types.Podio;

namespace AktBob.Podio.Contracts;

public record PostCommentCommand(ItemId PodioItemId, string TextValue);