using AAK.Podio;
using AktBob.Podio.Contracts;
using Microsoft.Extensions.Configuration;

namespace AktBob.Podio.Handlers;
public class PostCommentHandler(IPodioFactory podioFactory, IConfiguration configuration) : IPostCommentHandler
{
    private readonly IPodioFactory _podioFactory = podioFactory;
    private readonly IConfiguration _configuration = configuration;

    public async Task Handle(PostCommentCommand command, CancellationToken cancellationToken)
    {
        var podio = _podioFactory.Create(
            appId: command.PodioItemId.AppId,
            appToken: ConfigurationHelper.GetAppToken(_configuration, command.PodioItemId.AppId),
            clientId: ConfigurationHelper.GetClientId(_configuration),
            clientSecret: ConfigurationHelper.GetClientSecret(_configuration));

        await podio.PostItemComment(command.PodioItemId.AppId, command.PodioItemId.Id, command.TextValue, cancellationToken);
    }
}
