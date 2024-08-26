using AktBob.CheckOCRScreeningStatus.DTOs;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using JNJ.MessageBus;
using AktBob.CheckOCRScreeningStatus.Events;
using Microsoft.Extensions.Configuration;
using AktBob.Queue.Contracts;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.CheckOCRScreeningStatus.BackgroundServices;
internal class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IEventBus _eventBus;
    private readonly IData _data;
    private readonly IConfiguration _configuration;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };


    public Worker(ILogger<Worker> logger, IEventBus eventBus, IData data, IConfiguration configuration, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _eventBus = eventBus;
        _data = data;
        _configuration = configuration;
        ServiceProvider = serviceProvider;
    }

    public IServiceProvider ServiceProvider { get; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var delay = _configuration.GetValue<int?>("CheckOCRScreeningStatus:QueuePollingIntervalSeconds") ?? 60;
        var maxMessages = _configuration.GetValue<int?>("CheckOCRScreeningStatus:QueueMaxMessages") ?? 10;
        var connectionString = _configuration.GetConnectionString("AzureStorage");
        var queueName = _configuration.GetValue<string>("CheckOCRScreeningStatus:QueueName");

        using (var scope = ServiceProvider.CreateScope())
        {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            while (!stoppingToken.IsCancellationRequested)
            {
                var getMessagesQuery = new GetQueueMessagesQuery(
                    connectionString!,
                    queueName!,
                    maxMessages);

                var messages = await mediator.Send(getMessagesQuery);
                
                if (messages.IsSuccess)
                {

                    foreach (var message in messages.Value)
                    {
                        var content = JsonSerializer.Deserialize<QueueMessageBodyDto>(message.Body, _jsonSerializerOptions);

                        if (content is not null)
                        {
                            _data.AddCase(content.FilArkivCaseId, content.PodioItemId);
                            await _eventBus.Publish(new CaseAdded(content.FilArkivCaseId));
                        }
                        else
                        {
                            _logger.LogError($"Queue message not valid. ({message.Body})");
                        }

                        _logger.LogInformation("Deleting queue message {id}", message.Id);

                        var deleteMessageCommand = new DeleteQueueMessageCommand(
                            connectionString!,
                            queueName!,
                            message.Id,
                            message.PopReceipt);

                        await mediator.Send(deleteMessageCommand);
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(delay), stoppingToken);
            }
        }
    }
}