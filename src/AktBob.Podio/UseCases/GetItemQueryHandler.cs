using AAK.Podio;
using AAK.Podio.Models;
using AktBob.Podio.Contracts;
using Ardalis.Result;
using MassTransit.Mediator;
using Microsoft.Extensions.Configuration;

namespace AktBob.Podio.UseCases;
public class GetItemQueryHandler(IPodioFactory podioFactory, IConfiguration configuration) : MediatorRequestHandler<GetItemQuery, Result<Item>>
{
    private readonly IPodioFactory _podioFactory = podioFactory;
    private readonly IConfiguration _configuration = configuration;

    protected override async Task<Result<Item>> Handle(GetItemQuery request, CancellationToken cancellationToken)
    {
        var podio = _podioFactory.Create(request.AppId, ConfigurationHelper.GetAppToken(_configuration, request.AppId), ConfigurationHelper.GetClientId(_configuration), ConfigurationHelper.GetClientSecret(_configuration));
        var item = await podio.GetItem(request.AppId, request.ItemId, cancellationToken);

        if (item == null)
        {
            return Result.NotFound();
        }

        return Result.Success(item);
    }
}
