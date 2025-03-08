using AAK.Podio;
using AAK.Podio.Models;
using AktBob.Podio.Contracts;
using AktBob.Shared;
using Microsoft.Extensions.Configuration;

namespace AktBob.Podio.Handlers;
internal class GetItemHandler(IPodioFactory podioFactory, IConfiguration configuration) : IGetItemHandler
{
    private readonly IPodioFactory _podioFactory = podioFactory;
    private readonly IConfiguration _configuration = configuration;

    public async Task<Result<Item>> Handle(PodioItemId podioItemId, CancellationToken cancellationToken)
    {
        var podio = _podioFactory.Create(
            appId: podioItemId.AppId, 
            appToken: ConfigurationHelper.GetAppToken(_configuration, podioItemId.AppId), 
            clientId: ConfigurationHelper.GetClientId(_configuration),
            clientSecret: ConfigurationHelper.GetClientSecret(_configuration));

        var item = await podio.GetItem(podioItemId.AppId, podioItemId.Id, cancellationToken);
        if (item == null)
        {
            return Result.Error($"Error getting item {podioItemId} from Podio");
        }

        return item;
    }
}