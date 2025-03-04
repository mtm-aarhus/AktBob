using AAK.Podio.Models;
using AktBob.Podio.Contracts;
using AktBob.Podio.Jobs;
using AktBob.Shared;

namespace AktBob.Podio;

internal class Module(IJobDispatcher jobDispatcher, IGetItemHandler getItemHandler) : IPodioModule
{
    public async Task<Result<Item>> GetItem(int appId, long itemId, CancellationToken cancellationToken) => await getItemHandler.Handle(appId, itemId, cancellationToken);

    public void PostComment(int appId, long itemId, string textValue) => jobDispatcher.Dispatch(new PostCommentJob(appId, itemId, textValue));

    public void UpdateTextField(int appId, long itemId, int fieldId, string textValue) => jobDispatcher.Dispatch(new UpdateTextFieldJob(appId, itemId, fieldId, textValue));
}
