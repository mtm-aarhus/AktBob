using AktBob.Queue.Contracts;
using Ardalis.GuardClauses;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AktBob.PodioHookProcessor.UseCases.MoveToFilArkivTrigger;
internal class BackgroundWorker : BackgroundService
{
    private ILogger<BackgroundWorker> _logger;
    private IConfiguration _configuration;

    public IServiceProvider ServiceProvider { get; }

    public BackgroundWorker(ILogger<BackgroundWorker> logger, IConfiguration configuration, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _configuration = configuration;
        ServiceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var tenancyName = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("UiPath:TenancyName"));
        var azureQueueName = Guard.Against.NullOrEmpty(_configuration.GetValue<string>($"OCRScreeningTrigger:{tenancyName}:AzureQueueName"));
        var uiPathQueueName = Guard.Against.NullOrEmpty(_configuration.GetValue<string>($"OCRScreeningTrigger:{tenancyName}:UiPathQueueName"));
        var delay = _configuration.GetValue<int?>("OCRScreeningTrigger:WorkerIntervalSeconds") ?? 10;

        using (var scope = ServiceProvider.CreateScope())
        {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            while (!stoppingToken.IsCancellationRequested)
            {
                var getQueueMessagesQuery = new GetQueueMessagesQuery(azureQueueName!);
                var azureQueueMessages = await mediator.Send(getQueueMessagesQuery);

                if (azureQueueMessages.IsSuccess)
                {

                    foreach (var azureQueueMessage in azureQueueMessages.Value)
                    {
                        var dokumentQueueItem = JsonSerializer.Deserialize<AzureQueueItemDto>(azureQueueMessage.Body);

                        if (dokumentQueueItem == null)
                        {
                            _logger.LogError("Azure queue item body does not match type of '{type}'", typeof(AzureQueueItemDto));
                            continue;
                        }

                        // 

                        var deleteAzureQueueItemCommand = new DeleteQueueMessageCommand(azureQueueName, azureQueueMessage.Id, azureQueueMessage.PopReceipt);
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(delay));
            }
        }
    }
}
