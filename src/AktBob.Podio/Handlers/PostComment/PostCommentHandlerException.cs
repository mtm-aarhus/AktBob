using System.Net;
using AktBob.Shared.Types.Podio;
using Microsoft.Extensions.Logging;

namespace AktBob.Podio.Handlers.PostComment;

internal class PostCommentHandlerException : IPostCommentHandler
{
    private readonly IPostCommentHandler _next;
    private readonly ILogger<PostCommentHandler> _logger;

    public PostCommentHandlerException(IPostCommentHandler next, ILogger<PostCommentHandler> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task<ErrorOr<Success>> Handle(ItemId itemId, string textValue, CancellationToken cancellationToken)
    {
        try
        {
            return await _next.Handle(itemId, textValue, cancellationToken);
        }
        catch (HttpRequestException ex) when (ex.StatusCode is HttpStatusCode.NotFound)
        {
            _logger.LogError("Cannot post comment on Podio Item {id}. Item not found.", itemId);
            return Error.NotFound("Podio.ItemNotFound", $"Podio Item {itemId} not found.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(PostCommentHandler));
            return Error.Failure("PostCommentHandler.Failure", ex.Message);
        }
    }
}