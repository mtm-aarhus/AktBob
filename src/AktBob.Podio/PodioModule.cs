using AAK.Podio.Models;
using AktBob.Podio.Contracts;
using AktBob.Podio.Handlers.GetItem;
using AktBob.Podio.Jobs;
using AktBob.Shared;
using AktBob.Shared.Types.Podio;

namespace AktBob.Podio;

internal class PodioModule(IJobDispatcher jobDispatcher, IGetItemHandler getItemHandler) : IPodioModule
{
    public async Task<ErrorOr<Item>> GetItem(ItemId podioItemId, CancellationToken cancellationToken) => await getItemHandler.Handle(podioItemId, cancellationToken);
    public void PostComment(ItemId itemId, string textValue)=> jobDispatcher.Dispatch(new PostCommentJob(itemId, textValue));
    public void UpdateTextField(ItemId itemId, int fieldId, string textValue) => jobDispatcher.Dispatch(new UpdateTextFieldJob(itemId, fieldId, textValue));
}
