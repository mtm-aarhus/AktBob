using AktBob.OpenOrchestrator.Contracts;

namespace AktBob.JobHandlers.Handlers;
internal class CreateOpenOrchestratorQueueItem(IServiceScopeFactory serviceScopeFactory, ILogger<CreateOpenOrchestratorQueueItem> logger)
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly ILogger<CreateOpenOrchestratorQueueItem> _logger = logger;

    public async Task Run(string queueName, string reference, string payload, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Createing OpenOrchestrator queue item ...");
        _logger.LogInformation("Queue name: '{name}'", queueName);
        _logger.LogInformation("Reference: '{reference}'", reference);
        _logger.LogInformation("Payload: {payload}", payload.ToString());

        using var scope = _serviceScopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var command = new CreateQueueItemCommand(queueName, payload, reference);
        await mediator.Send(command, cancellationToken);
    }
}