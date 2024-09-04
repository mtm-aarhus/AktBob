using AktBob.DatabaseAPI.Contracts;
using AktBob.Queue.Contracts;
using Ardalis.GuardClauses;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AktBob.PodioHookProcessor.UseCases.RegisterCase;
internal class BackgroundWorker : BackgroundService
{
    private readonly ILogger<BackgroundWorker> _logger;
    private readonly IConfiguration _configuration;

    public BackgroundWorker(ILogger<BackgroundWorker> logger, IConfiguration configuration, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _configuration = configuration;
        ServiceProvider = serviceProvider;
    }

    public IServiceProvider ServiceProvider { get; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var azureQueueName = Guard.Against.NullOrEmpty(_configuration.GetValue<string>($"RegisterCase:AzureQueueName"));
        var delay = _configuration.GetValue<int?>("RegisterCase:WorkerIntervalSeconds") ?? 10;

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
                        var deleteAzureQueueItemCommand = new DeleteQueueMessageCommand(azureQueueName, azureQueueMessage.Id, azureQueueMessage.PopReceipt);
                        await mediator.Send(deleteAzureQueueItemCommand);

                        if (string.IsNullOrEmpty(azureQueueMessage.Body))
                        {
                            _logger.LogError("Azure queue item body is empty. Expected a Podio item Id");
                            continue;
                        }

                        var messageContent = JsonSerializer.Deserialize<AzureQueueItemDto>(azureQueueMessage.Body);

                        if (messageContent == null)
                        {
                            _logger.LogError("Azure queue item not valid.");
                            continue;
                        }

                        var deskproId = Convert.ToInt32(messageContent.DeskproId);
                        var podioItemId = Convert.ToInt64(messageContent.PodioItemId);

                        var ticketQuery = new GetTicketByDeskproIdQuery(deskproId);
                        var ticketResult = await mediator.Send(ticketQuery);
                        
                        if (!ticketResult.IsSuccess || ticketResult.Value.Count() == 0)
                        {
                            _logger.LogWarning("No tickets found in database for DeskproId '{deskproId}'", deskproId);
                            continue;
                        }

                        if (ticketResult.Value.Count() > 1)
                        {
                            _logger.LogWarning("{count} tickets found in database for DeskproId '{deskproId}'", ticketResult.Value.Count(), deskproId);
                            continue;
                        }

                        var postCaseCommand = new PostCaseCommand(ticketResult.Value.First().Id, podioItemId, null);
                        var postCaseCommandResult = await mediator.Send(postCaseCommand);

                        if (!postCaseCommandResult.IsSuccess)
                        {
                            _logger.LogError("Error adding case to database");
                        }
                    }
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(delay));
        }
    }
}
