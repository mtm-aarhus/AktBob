using AktBob.Podio.Contracts;
using AktBob.Shared;
using Ardalis.GuardClauses;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.Podio.Jobs;

internal record PostCommentJob(PodioItemId PodioItemId, string TextValue);

internal class PostComment(IServiceScopeFactory serviceScopeFactory) : IJobHandler<PostCommentJob>
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    public async Task Handle(PostCommentJob job, CancellationToken cancellationToken = default)
    {
        Guard.Against.NegativeOrZero(job.PodioItemId.AppId);
        Guard.Against.NegativeOrZero(job.PodioItemId.Id);
        Guard.Against.NullOrEmpty(job.TextValue);

        using var scope = _serviceScopeFactory.CreateScope();
        var postPodioCommentHandler = scope.ServiceProvider.GetRequiredService<IPostCommentHandler>();

        var command = new PostCommentCommand(job.PodioItemId, job.TextValue);
        await postPodioCommentHandler.Handle(command, cancellationToken);
    }
}