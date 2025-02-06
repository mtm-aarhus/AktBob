using AAK.Podio;
using AktBob.Podio.Contracts;
using MassTransit.Mediator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AktBob.Podio.UseCases;
public class PostItemCommentCommandHandler(IPodioFactory podioFactory, IConfiguration configuration, ILogger<PostItemCommentCommandHandler> logger) : MediatorRequestHandler<PostItemCommentCommand>
{
    private readonly IPodioFactory _podioFactory = podioFactory;
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<PostItemCommentCommandHandler> _logger = logger;

    protected override async Task Handle(PostItemCommentCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Posting comment on Podio item. ItemId {itemId} Value: '{value}'", command.ItemId, command.Comment);

        var podio = _podioFactory.Create(command.AppId, ConfigurationHelper.GetAppToken(_configuration, command.AppId), ConfigurationHelper.GetClientId(_configuration), ConfigurationHelper.GetClientSecret(_configuration));
        await podio.PostItemComment(command.AppId, command.ItemId, command.Comment, cancellationToken);

        _logger.LogInformation("Comment posted on Podio item. ItemId {itemId} Value: '{value}'", command.ItemId, command.Comment);
    }
}
