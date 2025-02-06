using AAK.Podio;
using AAK.Podio.Models;
using AktBob.Podio.Contracts;
using Ardalis.Result;
using MassTransit.Mediator;

namespace AktBob.Podio.UseCases;
public class GetItemQueryHandler(IPodio podio) : MediatorRequestHandler<GetItemQuery, Result<Item>>
{
    private readonly IPodio _podio = podio;

    protected override async Task<Result<Item>> Handle(GetItemQuery request, CancellationToken cancellationToken) => await _podio.GetItem(request.AppId, request.ItemId, cancellationToken);
}
