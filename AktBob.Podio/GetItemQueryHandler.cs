using AAK.Podio;
using AAK.Podio.Models;
using AktBob.Podio.Contracts;
using Ardalis.Result;
using MediatR;

namespace AktBob.Podio;
internal class GetItemQueryHandler(IPodio podio) : IRequestHandler<GetItemQuery, Result<Item>>
{
    private readonly IPodio _podio = podio;

    public async Task<Result<Item>> Handle(GetItemQuery request, CancellationToken cancellationToken) => await _podio.GetItem(request.AppId, request.ItemId, cancellationToken);
}
