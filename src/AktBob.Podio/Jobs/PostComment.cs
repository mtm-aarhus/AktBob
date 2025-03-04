using AktBob.Podio.Contracts;
using AktBob.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.Podio.Jobs;

internal record PostCommentJob(int AppId, long ItemId, string TextValue);

internal class PostComment(IServiceScopeFactory serviceScopeFactory) : IJobHandler<PostCommentJob>
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    public async Task Handle(PostCommentJob job, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var postPodioCommentHandler = scope.ServiceProvider.GetRequiredService<IPostCommentHandler>();
        await postPodioCommentHandler.Handle(job.AppId, job.ItemId, job.TextValue, cancellationToken);
    }
}