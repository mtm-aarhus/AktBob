using Ardalis.Result;
using AAK.Podio.Models;
using AktBob.Shared;

namespace AktBob.Podio.Contracts;

public interface IPodioModule
{
    Task<Result<Item>> GetItem(PodioItemId podioItemId, CancellationToken cancellationToken);
    void PostComment(PostCommentCommand command);
    void UpdateTextField(UpdateTextFieldCommand command);
}