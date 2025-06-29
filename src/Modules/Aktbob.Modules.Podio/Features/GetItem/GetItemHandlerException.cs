using System.Net;
using AktBob.Shared.Contracts.Modules.Podio;
using AktBob.Shared.Types.Podio;
using Microsoft.Extensions.Logging;

namespace Aktbob.Modules.Podio.Features.GetItem;

internal class GetItemHandlerException(IGetItemHandler next, ILogger<GetItemHandler> logger) : IGetItemHandler
{
    public async Task<ErrorOr<ItemDto>> Handle(ItemId itemId, CancellationToken cancellationToken)
    {
        try
        {
            return await next.Handle(itemId, cancellationToken);
        }
        catch (HttpRequestException ex)
        when (ex.StatusCode is HttpStatusCode.NotFound)
        {
            logger.LogWarning("Item {itemId} not found.", itemId);
            return Error.NotFound("Podio.ItemNotFound", $"Item {itemId} not found.");    
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in {name}", nameof(GetItemHandler));
            return Error.Failure("GetItemHandler.Failure", ex.Message);
        }
    }
}