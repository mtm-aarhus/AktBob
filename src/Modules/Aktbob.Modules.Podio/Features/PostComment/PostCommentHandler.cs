using AAK.Podio;
using AktBob.Shared.Types.Podio;

namespace Aktbob.Modules.Podio.Features.PostComment;
public class PostCommentHandler(IPodioFactory podioFactory, IConfigurationHelper configurationHelper) : IPostCommentHandler
{
    public async Task<ErrorOr<Success>> Handle(ItemId itemId, string textValue, CancellationToken cancellationToken)
    {
        var podio = podioFactory.Create(
            appId: itemId.AppId, 
            appToken: configurationHelper.GetAppToken(itemId.AppId), 
            clientId: configurationHelper.ClientId,
            clientSecret: configurationHelper.GetClientSecret);

        await podio.PostItemComment(itemId.AppId, itemId.Id, textValue, cancellationToken);
        return Result.Success;
    }
}
