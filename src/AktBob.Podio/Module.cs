using AAK.Podio.Models;
using AktBob.Podio.Contracts;
using AktBob.Podio.Jobs;
using AktBob.Shared;

namespace AktBob.Podio;

internal class Module(IJobDispatcher jobDispatcher, IGetItemHandler getItemHandler) : IPodioModule
{
    public async Task<Result<Item>> GetItem(PodioItemId podioItemId, CancellationToken cancellationToken) => await getItemHandler.Handle(podioItemId, cancellationToken);

    public void PostComment(PostCommentCommand command) => jobDispatcher.Dispatch(new PostCommentJob(command.PodioItemId, command.TextValue));

    public void UpdateTextField(UpdateTextFieldCommand command) => jobDispatcher.Dispatch(new UpdateTextFieldJob(command.PodioItemId, command.FieldId, command.TextValue));
}
