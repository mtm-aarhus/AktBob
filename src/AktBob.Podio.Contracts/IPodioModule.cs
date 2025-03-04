using Ardalis.Result;
using AAK.Podio.Models;

namespace AktBob.Podio.Contracts;

public interface IPodioModule
{
    Task<Result<Item>> GetItem(int appId, long itemId, CancellationToken cancellationToken);
    void PostComment(int appId, long itemId, string textValue);
    void UpdateTextField(int appId, long itemId, int fieldId, string textValue);
}
