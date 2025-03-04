using AAK.Podio;
using AAK.Podio.Models;
using AktBob.Podio.Contracts;
using Microsoft.Extensions.Configuration;

namespace AktBob.Podio.Handlers;
internal class GetItemHandler(IPodioFactory podioFactory, IConfiguration configuration) : IGetItemHandler
{
    private readonly IPodioFactory _podioFactory = podioFactory;
    private readonly IConfiguration _configuration = configuration;

    public async Task<Result<Item>> Handle(int appId, long itemId, CancellationToken cancellationToken)
    {
        var podio = _podioFactory.Create(
            appId: appId, 
            appToken: ConfigurationHelper.GetAppToken(_configuration, appId), 
            clientId: ConfigurationHelper.GetClientId(_configuration),
            clientSecret: ConfigurationHelper.GetClientSecret(_configuration));

        var item = await podio.GetItem(appId, itemId, cancellationToken);

        if (item == null)
        {
            return Result.NotFound();
        }

        return Result.Success(item);
    }
}