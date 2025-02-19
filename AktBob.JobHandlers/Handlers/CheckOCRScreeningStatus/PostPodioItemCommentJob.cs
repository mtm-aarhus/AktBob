using AktBob.Podio.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.JobHandlers.Handlers.CheckOCRScreeningStatus;
internal class PostPodioItemCommentJob(IConfiguration configuration, IServiceScopeFactory serviceScopeFactory)
{
    private readonly IConfiguration _configuration = configuration;
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    public async Task Run(long podioItemId, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var podioAppId = Guard.Against.Null(_configuration.GetValue<int>("Podio:AppId"));
        var commentText = "OCR screening af dokumenterne i FilArkiv er færdig.";

        var postCommentCommand = new PostItemCommentCommand(podioAppId, podioItemId, commentText);
        await mediator.Send(postCommentCommand, cancellationToken);
    }
}
