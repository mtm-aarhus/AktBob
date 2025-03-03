using AktBob.Podio.Contracts;
using AktBob.Podio.Contracts.Jobs;
using AktBob.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.Podio.JobHandlers;
internal class PostComment(IServiceScopeFactory serviceScopeFactory) : IJobHandler<PostCommentJob>
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    public async Task Handle(PostCommentJob job, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var postPodioCommentHandler = scope.ServiceProvider.GetRequiredService<IPostPodioItemCommentHandler>();
        await postPodioCommentHandler.Handle(job.AppId, job.ItemId, job.TextValue, cancellationToken);
    }
}