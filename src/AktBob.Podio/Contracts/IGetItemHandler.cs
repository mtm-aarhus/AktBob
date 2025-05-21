using AAK.Podio.Models;
using AktBob.Shared.Types.Podio;

namespace AktBob.Podio.Contracts;
internal interface IGetItemHandler
{
    Task<Result<Item>> Handle(ItemId podioItemId, CancellationToken cancellationToken);
}