using AktBob.Queue.Contracts;
using AktBob.UiPath.Contracts;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AktBob.Aktliste;
internal class Worker : BackgroundService
{
    private ILogger<Worker> _logger;
    private IConfiguration _configuration;

    public IServiceProvider ServiceProvider { get; }

    public Worker(ILogger<Worker> logger, IConfiguration configuration, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _configuration = configuration;
        ServiceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var connectionString = _configuration.GetConnectionString("AzureStorage");
        var azureQueueName = _configuration.GetValue<string>("AktlisteModule:AzureQueueName");
        var uiPathQueueName = _configuration.GetValue<string>("AktlisteModule:UiPathQueueName");

        using (var scope = ServiceProvider.CreateScope())
        {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            while (!stoppingToken.IsCancellationRequested)
            {
                var getQueueMessagesQuery = new GetQueueMessagesQuery(connectionString!, azureQueueName!);
                var azureQueueMessages = await mediator.Send(getQueueMessagesQuery);

                if (azureQueueMessages.IsSuccess)
                {

                    foreach (var azureQueueMessage in azureQueueMessages.Value)
                    {
                        var aktlisteQueueItem = JsonSerializer.Deserialize<AktlisteQueueItem>(azureQueueMessage.Body);

                        if (aktlisteQueueItem == null)
                        {
                            _logger.LogError("Azure queue item body does not match type of '{type}'", typeof(AktlisteQueueItem));
                            continue;
                        }

                        var addUiPathQueueItemCommand = new AddQueueItemCommand(uiPathQueueName!, azureQueueMessage.Id, aktlisteQueueItem);
                        await mediator.Send(addUiPathQueueItemCommand);

                        var deleteAzureQueueItemCommand = new DeleteQueueMessageCommand(connectionString, azureQueueName, azureQueueMessage.Id, azureQueueMessage.PopReceipt);
                    }
                }

            }
        }
    }
}
