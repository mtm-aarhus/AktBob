using AAK.Podio.Models;
using Ardalis.Result;

namespace AktBob.Podio.Contracts;
public interface IGetPodioItemHandler
{
    Task<Result<Item>> Handle(int appId, long itemId, CancellationToken cancellationToken);
}