using AktBob.Shared;
using AktBob.Shared.Exceptions;
using AktBob.Shared.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text;

namespace AktBob.OpenOrchestrator;

internal record CreateQueueItemJob(string QueueName, string Reference, string Payload);

internal class CreateQueueItem(IServiceScopeFactory serviceScopeFactory, ILogger<CreateQueueItem> logger) : IJobHandler<CreateQueueItemJob>
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly ILogger<CreateQueueItem> _logger = logger;

    public async Task Handle(CreateQueueItemJob job, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredServiceOrThrow<ICreateQueueItemHandler>();
                
        var bytes = Convert.FromBase64String(job.Payload);
        var decodedPayload = Encoding.UTF8.GetString(bytes);

        var result = await handler.Handle(job.QueueName, decodedPayload, job.Reference, cancellationToken);
        if (!result.IsSuccess) throw new BusinessException($"Unable to create OpenOrchestrator queue item: {result.Errors.AsString()}");
        
        _logger.LogInformation("OpenOrchestrator queue item {id} created. Queue: {queueName}, Reference:_{reference}, Payload: {payload}", result.Value, job.QueueName, job.Reference, decodedPayload);
    }
}