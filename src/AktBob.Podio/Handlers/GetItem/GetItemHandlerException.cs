using System.Net;
using AAK.Podio.Models;
using AktBob.Shared.Types.Podio;
using Microsoft.Extensions.Logging;

namespace AktBob.Podio.Handlers.GetItem;

internal class GetItemHandlerException : IGetItemHandler
{
    private readonly IGetItemHandler _next;
    private readonly ILogger<GetItemHandler> _logger;

    public GetItemHandlerException(IGetItemHandler next, ILogger<GetItemHandler> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task<ErrorOr<Item>> Handle(ItemId itemId, CancellationToken cancellationToken)
    {
        try
        {
            return await _next.Handle(itemId, cancellationToken);
        }
        catch (HttpRequestException ex)
        when (ex.StatusCode is HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Item {itemId} not found.", itemId);
            return Error.NotFound("Podio.ItemNotFound", $"Item {itemId} not found.");    
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(GetItemHandler));
            return Error.Failure("GetItemHandler.Failure", ex.Message);
        }
    }
}