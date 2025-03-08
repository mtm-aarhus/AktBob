using AAK.Podio.Models;
using AktBob.Shared;

namespace AktBob.Podio.Contracts;
internal interface IGetItemHandler
{
    Task<Result<Item>> Handle(PodioItemId podioItemId, CancellationToken cancellationToken);
}