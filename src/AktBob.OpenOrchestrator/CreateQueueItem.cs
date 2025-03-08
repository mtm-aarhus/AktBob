using AktBob.Shared;
using AktBob.Shared.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AktBob.OpenOrchestrator;

internal record CreateQueueItemJob(string QueueName, string Reference, string Payload);

internal class CreateQueueItem(IServiceScopeFactory serviceScopeFactory, ILogger<CreateQueueItem> logger) : IJobHandler<CreateQueueItemJob>
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly ILogger<CreateQueueItem> _logger = logger;

    public async Task Handle(CreateQueueItemJob job, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICreateQueueItemHandler>();

        _logger.LogInformation("Creating OpenOrchestrator queue item. {job}", job);

        var result = await handler.Handle(job.QueueName, job.Payload, job.Reference, cancellationToken);

        if (result.IsSuccess)
        {
            _logger.LogInformation("OpenOrchestrator queue item {id} created. Job: {job}", result.Value, job);
        }
        else
        {
            _logger.LogError(result.Errors.AsString());
        }
    }
}