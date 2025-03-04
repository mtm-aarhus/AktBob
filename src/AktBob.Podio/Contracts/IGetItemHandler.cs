using AAK.Podio.Models;

namespace AktBob.Podio.Contracts;
internal interface IGetItemHandler
{
    Task<Result<Item>> Handle(int appId, long itemId, CancellationToken cancellationToken);
}