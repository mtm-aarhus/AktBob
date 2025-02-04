using AktBob.CheckOCRScreeningStatus.DTOs;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using AktBob.CheckOCRScreeningStatus.Events;
using Microsoft.Extensions.Configuration;
using AktBob.Queue.Contracts;
using Microsoft.Extensions.DependencyInjection;
using MassTransit.Mediator;
using MassTransit;

namespace AktBob.CheckOCRScreeningStatus.BackgroundServices;
internal class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IBus _bus;
    private readonly IData _data;
    private readonly IConfiguration _configuration;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };


    public Worker(ILogger<Worker> logger, IBus bus, IData data, IConfiguration configuration, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _bus = bus;
        _data = data;
        _configuration = configuration;
        ServiceProvider = serviceProvider;
    }

    public IServiceProvider ServiceProvider { get; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var delay = _configuration.GetValue<int?>("CheckOCRScreeningStatus:QueuePollingIntervalSeconds") ?? 60;
        var maxMessages = _configuration.GetValue<int?>("CheckOCRScreeningStatus:QueueMaxMessages") ?? 10;
        var queueName = _configuration.GetValue<string>("CheckOCRScreeningStatus:QueueName");

        using (var scope = ServiceProvider.CreateScope())
        {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            while (!stoppingToken.IsCancellationRequested)
            {
                var getMessagesQuery = new GetQueueMessagesQuery(queueName!, maxMessages);
                var messages = await mediator.SendRequest(getMessagesQuery);
                
                if (messages.IsSuccess)
                {
                    foreach (var message in messages.Value)
                    {
                        // Retrieve the Base64 encoded message from Azure Queue
                        string base64EncodedMessage = message.Body.ToString();

                        // Decode the Base64 message back to a JSON string
                        string base64decodedMessage = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(base64EncodedMessage));

                        var content = JsonSerializer.Deserialize<QueueMessageBodyDto>(base64decodedMessage, _jsonSerializerOptions);

                        if (content is not null)
                        {
                            _data.AddCase(content.FilArkivCaseId, content.PodioItemId);
                            await _bus.Publish(new CaseAdded(content.FilArkivCaseId));
                        }
                        else
                        {
                            _logger.LogError($"Queue message not valid. ({message.Body})");
                        }

                        _logger.LogInformation("Deleting queue message {id}", message.Id);

                        var deleteMessageCommand = new DeleteQueueMessageCommand(
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