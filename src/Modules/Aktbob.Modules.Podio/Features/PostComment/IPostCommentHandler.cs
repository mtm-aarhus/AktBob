using AktBob.Shared.Types.Podio;

namespace Aktbob.Modules.Podio.Features.PostComment;
internal interface IPostCommentHandler
{
    Task<ErrorOr<Success>> Handle(ItemId itemId, string textValue, CancellationToken cancellationToken);
}