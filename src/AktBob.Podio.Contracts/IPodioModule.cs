using Ardalis.Result;
using AAK.Podio.Models;
using AktBob.Shared.Types.Podio;

namespace AktBob.Podio.Contracts;

public interface IPodioModule
{
    Task<Result<Item>> GetItem(ItemId podioItemId, CancellationToken cancellationToken);
    void PostComment(PostCommentCommand command);
    void UpdateTextField(UpdateTextFieldCommand command);
}