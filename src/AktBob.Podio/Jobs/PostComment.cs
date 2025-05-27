using AktBob.Podio.Handlers.PostComment;
using AktBob.Shared;
using AktBob.Shared.Extensions;
using AktBob.Shared.Types.Podio;
using Ardalis.GuardClauses;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.Podio.Jobs;

internal record PostCommentJob(ItemId ItemId, string TextValue);

internal class PostComment(IServiceScopeFactory serviceScopeFactory) : IJobHandler<PostCommentJob>
{
    public async Task Handle(PostCommentJob job, CancellationToken cancellationToken = default)
    {
        Guard.Against.NegativeOrZero(job.ItemId.AppId);
        Guard.Against.NegativeOrZero(job.ItemId.Id);
        Guard.Against.NullOrEmpty(job.TextValue);

        using var scope = serviceScopeFactory.CreateScope();
        var postPodioCommentHandler = scope.ServiceProvider.GetRequiredServiceOrThrow<IPostCommentHandler>();

        await postPodioCommentHandler.Handle(job.ItemId, job.TextValue, cancellationToken);
    }
}
