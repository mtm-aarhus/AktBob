using AktBob.DatabaseAPI.Contracts;
using AktBob.Podio.Contracts;
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
        var podioAppId = Guard.Against.Null(_configuration.GetValue<int?>("Podio:AppId"));
        var podioFields = Guard.Against.Null(Guard.Against.NullOrEmpty(_configuration.GetSection("Podio:Fields").GetChildren().ToDictionary(x => long.Parse(x.Key), x => x.Get<PodioField>())));
        var podioFieldDeskproId = Guard.Against.Null(podioFields.FirstOrDefault(x => x.Value.AppId == podioAppId && x.Value.Label == "DeskproId"));
        Guard.Against.Null(podioFieldDeskproId.Value);

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
                        _logger.LogInformation("RegisterCase: Processing queue item {queueId}: Message content: '{content}'", azureQueueMessage.Id, azureQueueMessage.Body);

                        var deleteAzureQueueItemCommand = new DeleteQueueMessageCommand(azureQueueName, azureQueueMessage.Id, azureQueueMessage.PopReceipt);
                        await mediator.Send(deleteAzureQueueItemCommand);

                        if (string.IsNullOrEmpty(azureQueueMessage.Body))
                        {
                            _logger.LogError("Azure queue item body is empty. Expected a Podio item Id");
                            continue;
                        }

                        var messageContent = JsonSerializer.Deserialize<string>(azureQueueMessage.Body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        if (!long.TryParse(messageContent, out long podioItemId))
                        {
                            _logger.LogError("Could not parse Azure queue item body as a 'long' data type. Body content: '{body}'", azureQueueMessage.Body);
                            continue;
                        }

                        // Get metadata from Podio
                        var getPodioItemQuery = new GetItemQuery(podioAppId, podioItemId);
                        var getPodioItemQueryResult = await mediator.Send(getPodioItemQuery, stoppingToken);

                        if (!getPodioItemQueryResult.IsSuccess)
                        {
                            _logger.LogError("Could not get item {itemId} from Podio", podioItemId);
                            continue;
                        }

                        var deskproIdString = getPodioItemQueryResult.Value.Fields.FirstOrDefault(x => x.Id == podioFieldDeskproId.Key)?.Value?.FirstOrDefault();
                        if (string.IsNullOrEmpty(deskproIdString))
                        {
                            _logger.LogError("Could not get Deskpro Id field value from Podio Item {itemId}", podioItemId);
                            continue;
                        }

                        if (!int.TryParse(deskproIdString, out int deskproId))
                        {
                            _logger.LogError("Could not parse Deskpro Id field value as integer from Podio Item {itemId}", podioItemId);
                            continue;
                        }

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

                        _logger.LogInformation("RegisterCase: Queue item {queueId} processed", azureQueueMessage.Id);

                    }
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(delay));
        }
    }
}
