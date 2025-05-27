using AAK.Podio.Models;
using AktBob.Shared.Types.Podio;

namespace AktBob.Podio.Contracts;

public interface IPodioModule
{
    Task<ErrorOr<Item>> GetItem(ItemId itemId, CancellationToken cancellationToken);
    void PostComment(ItemId itemId, string textValue);
    void UpdateTextField(ItemId itemId, int fieldId, string textValue);
}