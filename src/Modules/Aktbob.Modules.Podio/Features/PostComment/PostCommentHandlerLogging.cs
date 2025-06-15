using AktBob.Shared.Extensions;
using AktBob.Shared.Types.Podio;

namespace Aktbob.Modules.Podio.Features.PostComment;

internal class PostCommentHandlerLogging(IPostCommentHandler next, ILogger<PostCommentHandler> logger)
    : IPostCommentHandler
{
    public async Task<ErrorOr<Success>> Handle(ItemId itemId, string textValue, CancellationToken cancellationToken)
    {
        logger.LogInformation("Posting comment on Podio Item {itemId}: '{textValue}'", itemId, textValue);
        
        var result = await next.Handle(itemId, textValue, cancellationToken);
        
        result.Switch(
            _ => logger.LogInformation("Comment posted on Podio item {id}", itemId),
            errors => logger.LogWarning("{name}: {errors}", nameof(PostCommentHandler), errors.ToCommaDelimitedString()));
        
        return result;
    }
}