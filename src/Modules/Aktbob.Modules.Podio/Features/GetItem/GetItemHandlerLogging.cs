using AktBob.Shared.Contracts.Modules.Podio;
using AktBob.Shared.Extensions;
using AktBob.Shared.Types.Podio;
using Microsoft.Extensions.Logging;

namespace Aktbob.Modules.Podio.Features.GetItem;

internal class GetItemHandlerLogging(IGetItemHandler next, ILogger<GetItemHandler> logger) : IGetItemHandler
{
    public async Task<ErrorOr<ItemDto>> Handle(ItemId itemId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting Podio item {podioItemId}", itemId);

        var result = await next.Handle(itemId, cancellationToken);
        
        result.Switch(
            _ => logger.LogInformation("Podio item {id} retrieved", itemId),
            errors => logger.LogWarning("{name}: {errors}", nameof(GetItemHandler), errors.ToCommaDelimitedString()));
        
        return result;
    }
}