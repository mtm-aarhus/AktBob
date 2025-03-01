using AAK.Podio;
using AktBob.Podio.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AktBob.Podio.Handlers;
public class PostPodioItemCommentHandler(IPodioFactory podioFactory, IConfiguration configuration, ILogger<PostPodioItemCommentHandler> logger) : IPostPodioItemCommentHandler
{
    private readonly IPodioFactory _podioFactory = podioFactory;
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<PostPodioItemCommentHandler> _logger = logger;

    public async Task Handle(int appId, long itemId, string comment, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Posting comment on Podio item. ItemId {itemId} Value: '{value}'", itemId, comment);

        var podio = _podioFactory.Create(
            appId: appId,
            appToken: ConfigurationHelper.GetAppToken(_configuration, appId),
            clientId: ConfigurationHelper.GetClientId(_configuration),
            clientSecret: ConfigurationHelper.GetClientSecret(_configuration));

        await podio.PostItemComment(appId, itemId, comment, cancellationToken);

        _logger.LogInformation("Comment posted on Podio item. ItemId {itemId} Value: '{value}'", itemId, comment);
    }
}
