using AAK.Podio;
using AAK.Podio.Models;
using AktBob.Shared.Types.Podio;
using Microsoft.Extensions.Configuration;

namespace AktBob.Podio.Handlers.GetItem;
internal class GetItemHandler(IPodioFactory podioFactory, IConfiguration configuration) : IGetItemHandler
{
    public async Task<ErrorOr<Item>> Handle(ItemId itemId, CancellationToken cancellationToken)
    {
        var podio = podioFactory.Create(
            appId: itemId.AppId, 
            appToken: ConfigurationHelper.GetAppToken(configuration, itemId.AppId), 
            clientId: ConfigurationHelper.GetClientId(configuration),
            clientSecret: ConfigurationHelper.GetClientSecret(configuration));

        var item = await podio.GetItem(itemId.AppId, itemId.Id, cancellationToken);
        if (item == null)
        {
            return Error.NotFound("Podio.ItemNotFound", $"Item {itemId} not found.");
        }

        return item;
    }
}