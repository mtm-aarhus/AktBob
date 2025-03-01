using AktBob.UiPath.Contracts;

namespace AktBob.JobHandlers.Handlers;
internal class CreateUiPathQueueItem(IServiceScopeFactory serviceScopeFactory, ILogger<CreateDocumentListQueueItemJobHandler> logger)
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly ILogger<CreateDocumentListQueueItemJobHandler> _logger = logger;

    public async Task Run(string queueName, string reference, string payload, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICreateUiPathQueueItemHandler>();

        _logger.LogInformation("Creating UiPath queue item ...");
        _logger.LogInformation("Queue name: '{name}'", queueName);
        _logger.LogInformation("Reference: '{reference}'", reference);
        _logger.LogInformation("Payload: {payload}", payload.ToString());

        await handler.Handle(queueName, reference, payload, cancellationToken);
    }
}