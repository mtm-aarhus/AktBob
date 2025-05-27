using AktBob.Shared.Extensions;
using AktBob.Shared.Types.Podio;
using Microsoft.Extensions.Logging;

namespace AktBob.Podio.Handlers.PostComment;

internal class PostCommentHandlerLogging : IPostCommentHandler
{
    private readonly IPostCommentHandler _next;
    private readonly ILogger<PostCommentHandler> _logger;

    public PostCommentHandlerLogging(IPostCommentHandler next, ILogger<PostCommentHandler> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task<ErrorOr<Success>> Handle(ItemId itemId, string textValue, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Posting comment on Podio Item {itemId}: '{textValue}'", itemId, textValue);
        
        var result = await _next.Handle(itemId, textValue, cancellationToken);
        
        result.Switch(
            _ => _logger.LogInformation("Comment posted on Podio item {id}", itemId),
            errors => _logger.LogWarning("{name}: {errors}", nameof(PostCommentHandler), errors.ToCommaDelimitedString()));
        
        return result;
    }
}