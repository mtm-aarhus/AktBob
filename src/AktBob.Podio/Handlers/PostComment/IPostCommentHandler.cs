using AktBob.Shared.Types.Podio;

namespace AktBob.Podio.Handlers.PostComment;
internal interface IPostCommentHandler
{
    Task<ErrorOr<Success>> Handle(ItemId itemId, string textValue, CancellationToken cancellationToken);
}