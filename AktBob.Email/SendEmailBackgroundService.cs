using AktBob.Email.Contracts;
using AktBob.Email.UseCases.SendEmail;
using MassTransit.Mediator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AktBob.Email;
internal class SendEmailBackgroundService : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly IQueueService _queueService;
    private readonly ILogger<SendEmailBackgroundService> _logger;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    public SendEmailBackgroundService(IConfiguration configuration, IQueueService queueService, ILogger<SendEmailBackgroundService> logger, IServiceProvider serviceProvider)
    {
        _configuration = configuration;
        _queueService = queueService;
        _logger = logger;
        ServiceProvider = serviceProvider;
    }

    public IServiceProvider ServiceProvider { get; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(_configuration.GetValue<int>("EmailModule:IntervalSeconds"));

        using (var scope = ServiceProvider.CreateScope())
        {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            while (!stoppingToken.IsCancellationRequested)
            {
                var messages = await _queueService.Queue.GetMessages();

                foreach (var message in messages)
                {
                    var content = JsonSerializer.Deserialize<EmailMessageDto>(message.Body, _jsonSerializerOptions);

                    if (content is null)
                    {
                        _logger.LogError("Email queue message content not valid. Content: " + message.Body);
                        await _queueService.Queue.DeleteMessage(message.Id, message.PopReceipt, stoppingToken);
                        continue;
                    }

                    if (string.IsNullOrEmpty(content.To)
                        || string.IsNullOrEmpty(content.Subject)
                        || string.IsNullOrEmpty(content.Body))
                    {
                        // Queue message is not valid
                        _logger.LogError("Email queue message content not valid. Content: " + content.Body);
                        await _queueService.Queue.DeleteMessage(message.Id, message.PopReceipt, stoppingToken);
                        continue;
                    }

                    var sendEmailCommand = new SendEmailCommand(content.To, content.Subject, content.Body);
                    await mediator.Send(sendEmailCommand);

                    await _queueService.Queue.DeleteMessage(message.Id, message.PopReceipt, stoppingToken);
                }

                await Task.Delay(interval, stoppingToken);
            }
        }
    }
}
