using AAK.Podio;
using AktBob.Podio.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AktBob.Podio.Handlers;
public class PostCommentHandler(IPodioFactory podioFactory, IConfiguration configuration, ILogger<PostCommentHandler> logger) : IPostCommentHandler
{
    private readonly IPodioFactory _podioFactory = podioFactory;
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<PostCommentHandler> _logger = logger;

    public async Task Handle(PostCommentCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Posting comment on Podio item. ItemId {itemId} Value: '{value}'", command.PodioItemId.Id, command.TextValue);

        var podio = _podioFactory.Create(
            appId: command.PodioItemId.AppId,
            appToken: ConfigurationHelper.GetAppToken(_configuration, command.PodioItemId.AppId),
            clientId: ConfigurationHelper.GetClientId(_configuration),
            clientSecret: ConfigurationHelper.GetClientSecret(_configuration));

        await podio.PostItemComment(command.PodioItemId.AppId, command.PodioItemId.Id, command.TextValue, cancellationToken);

        _logger.LogInformation("Comment posted on Podio item. ItemId {itemId} Value: '{value}'", command.PodioItemId.Id, command.TextValue);
    }
}
