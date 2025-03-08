using AktBob.Shared;

namespace AktBob.Podio.Contracts;

public record PostCommentCommand(PodioItemId PodioItemId, string TextValue);