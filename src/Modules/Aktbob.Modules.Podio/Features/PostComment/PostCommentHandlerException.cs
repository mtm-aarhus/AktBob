using System.Net;
using AktBob.Shared.Types.Podio;
using Microsoft.Extensions.Logging;

namespace Aktbob.Modules.Podio.Features.PostComment;

internal class PostCommentHandlerException(IPostCommentHandler next, ILogger<PostCommentHandler> logger)
    : IPostCommentHandler
{
    public async Task<ErrorOr<Success>> Handle(ItemId itemId, string textValue, CancellationToken cancellationToken)
    {
        try
        {
            return await next.Handle(itemId, textValue, cancellationToken);
        }
        catch (HttpRequestException ex) when (ex.StatusCode is HttpStatusCode.NotFound)
        {
            logger.LogError("Cannot post comment on Podio Item {id}. Item not found.", itemId);
            return Error.NotFound("Podio.ItemNotFound", $"Podio Item {itemId} not found.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in {name}", nameof(PostCommentHandler));
            return Error.Failure("PostCommentHandler.Failure", ex.Message);
        }
    }
}