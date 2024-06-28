using AktBob.CheckOCRScreeningStatus.DTOs;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using JNJ.MessageBus;
using AktBob.CheckOCRScreeningStatus.Events;
using Microsoft.Extensions.Configuration;

namespace AktBob.CheckOCRScreeningStatus.BackgroundServices;
internal class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IQueueService _queueService;
    private readonly IEventBus _eventBus;
    private readonly IData _data;
    private readonly IConfiguration _configuration;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };


    public Worker(ILogger<Worker> logger, IQueueService queueService, IEventBus eventBus, IData data, IConfiguration configuration)
    {
        _logger = logger;
        _queueService = queueService;
        _eventBus = eventBus;
        _data = data;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var delay = _configuration.GetValue<int?>("CheckOCRScreeningStatus:QueuePollingIntervalSeconds") ?? 60;
        var maxMessages = _configuration.GetValue<int?>("CheckOCRScreeningStatus:QueueMaxMessages") ?? 10;

        while (!stoppingToken.IsCancellationRequested)
        {
            var messages = await _queueService.Queue.GetMessages(maxMessages);

            if (!messages.Any())
            {
                _logger.LogInformation("No messages pending");
            }

            foreach (var message in messages)
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
                await _queueService.Queue.DeleteMessage(message.Id, message.PopReceipt);
            }
            
            await Task.Delay(TimeSpan.FromSeconds(delay), stoppingToken);
        }

    }
}