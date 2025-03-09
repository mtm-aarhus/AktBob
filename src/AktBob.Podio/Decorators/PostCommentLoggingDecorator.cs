using AktBob.Podio.Jobs;
using AktBob.Shared;
using Microsoft.Extensions.Logging;

namespace AktBob.Podio.Decorators;

internal class PostCommentLoggingDecorator(IJobHandler<PostCommentJob> inner, ILogger<PostCommentLoggingDecorator> logger) : IJobHandler<PostCommentJob>
{
    private readonly IJobHandler<PostCommentJob> _inner = inner;
    private readonly ILogger<PostCommentLoggingDecorator> _logger = logger;

    public async Task Handle(PostCommentJob job, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting job: {name} ({job})", nameof(PostComment), job);
        await _inner.Handle(job, cancellationToken);
        _logger.LogInformation("Job finished: {name} ({job})", nameof(PostComment), job);
    }
}