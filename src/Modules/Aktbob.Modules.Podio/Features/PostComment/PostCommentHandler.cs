using AAK.Podio;
using AktBob.Shared.Types.Podio;

namespace Aktbob.Modules.Podio.Features.PostComment;
public class PostCommentHandler(IPodioFactory podioFactory, IConfiguration configuration) : IPostCommentHandler
{
    public async Task<ErrorOr<Success>> Handle(ItemId itemId, string textValue, CancellationToken cancellationToken)
    {
        var podio = podioFactory.Create(
            appId: itemId.AppId,
            appToken: ConfigurationHelper.GetAppToken(configuration, itemId.AppId),
            clientId: ConfigurationHelper.GetClientId(configuration),
            clientSecret: ConfigurationHelper.GetClientSecret(configuration));

        await podio.PostItemComment(itemId.AppId, itemId.Id, textValue, cancellationToken);
        return Result.Success;
    }
}
