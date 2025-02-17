using AktBob.UiPath.Contracts;
using MassTransit.Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AktBob.JobHandlers.Handlers;
internal class CreateUiPathQueueItem(IServiceScopeFactory serviceScopeFactory, ILogger<CreateDocumentListQueueItemJobHandler> logger)
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly ILogger<CreateDocumentListQueueItemJobHandler> _logger = logger;

    public async Task Run(string queueName, string reference, string payload, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        _logger.LogInformation("Creating UiPath queue item ...");
        _logger.LogInformation("Queue name: '{name}'", queueName);
        _logger.LogInformation("Reference: '{reference}'", reference);
        _logger.LogInformation("Payload: {payload}", payload.ToString());

        var command = new AddQueueItemCommand(queueName, reference, payload);
        await mediator.Send(command);
    }
}