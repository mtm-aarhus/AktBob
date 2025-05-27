using AktBob.Shared.Types.Podio;
using AAK.Podio.Models;

namespace AktBob.Podio.Handlers.GetItem;
internal interface IGetItemHandler
{
    Task<ErrorOr<Item>> Handle(ItemId itemId, CancellationToken cancellationToken);
}