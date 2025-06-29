using AktBob.Shared.Contracts.Modules.Podio;
using AktBob.Shared.Types.Podio;

namespace Aktbob.Modules.Podio.Features.GetItem;
public interface IGetItemHandler
{
    Task<ErrorOr<ItemDto>> Handle(ItemId itemId, CancellationToken cancellationToken);
}