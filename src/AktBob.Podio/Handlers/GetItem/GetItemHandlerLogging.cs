using AAK.Podio.Models;
using AktBob.Shared.Extensions;
using AktBob.Shared.Types.Podio;
using Microsoft.Extensions.Logging;

namespace AktBob.Podio.Handlers.GetItem;

internal class GetItemHandlerLogging : IGetItemHandler
{
    private readonly IGetItemHandler _next;
    private readonly ILogger<GetItemHandler> _logger;

    public GetItemHandlerLogging(IGetItemHandler next, ILogger<GetItemHandler> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task<ErrorOr<Item>> Handle(ItemId itemId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting Podio item {podioItemId}", itemId);

        var result = await _next.Handle(itemId, cancellationToken);
        
        result.Switch(
            _ => _logger.LogInformation("Podio item {id} retrieved", itemId),
            errors => _logger.LogWarning("{name}: {errors}", nameof(GetItemHandler), errors.ToCommaDelimitedString()));
        
        return result;
    }
}