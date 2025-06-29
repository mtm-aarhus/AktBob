using AktBob.Shared.Types.Podio;

namespace Aktbob.Modules.Podio.Features.PostComment;
public interface IPostCommentHandler
{
    Task<ErrorOr<Success>> Handle(ItemId itemId, string textValue, CancellationToken cancellationToken);
}