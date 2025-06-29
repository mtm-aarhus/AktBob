using Aktbob.Processors.SendEmail.Handler;
using AktBob.Shared;
using AktBob.Shared.Contracts.Processors;
using Ardalis.GuardClauses;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Aktbob.Processors.SendEmail;

internal class SendEmailBackgroundService(IConfiguration configuration, IServiceScopeFactory scopeFactory, ILogger<SendEmailBackgroundService> logger) : BackgroundService
{
    private ServiceBusProcessor? _processor;
    private ServiceBusClient? _client;
    private readonly string _connectionString = Guard.Against.NullOrEmpty(configuration.GetConnectionString("AzureServiceBus"));
    private readonly string _queueName = Guard.Against.NullOrEmpty(configuration.GetValue<string>("EmailModule:ServiceBusQueueName"));
    private readonly ServiceBusClientOptions _serviceBusClientOptions = new ServiceBusClientOptions
    {
        TransportType = ServiceBusTransportType.AmqpWebSockets
    };
    
    private async Task MessageHandler(ProcessMessageEventArgs args)
    {
        using var scope =  scopeFactory.CreateScope();
        var sendEmailHandler = scope.ServiceProvider.GetRequiredService<ISendEmailHandler>();
        logger.LogInformation("Received: {body}", args.Message.Body.ToString());
        
        var job = MessageDeserializer.Deserialize<NotificationJob>(args.Message);
        var body = HtmlHelper.GenerateHtml(job.Fields.ToDictionary(), $"EmailTemplates/{job.TemplateName}.html");
        
        sendEmailHandler.Handle(job.Recipient, job.Subject, body, bodyIsHtml: true);
        await args.CompleteMessageAsync(args.Message);
    }

    private Task ErrorHandler(ProcessErrorEventArgs args)
    {
        logger.LogError("Error handling email job: {error}", args.Exception.ToString());
        return Task.CompletedTask;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _client = new ServiceBusClient(_connectionString, _serviceBusClientOptions);
        _processor = _client.CreateProcessor(_queueName, new ServiceBusProcessorOptions());
        
        _processor.ProcessMessageAsync += MessageHandler; // Add handler to process messages
        _processor.ProcessErrorAsync += ErrorHandler; // Add handler to process any errors

        // Start processing
        await _processor.StartProcessingAsync(stoppingToken);
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Stopping email message processor...");

        if (_processor is not null)
        {
            try
            {
                await _processor.StopProcessingAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to stop the email processor cleanly.");
            }
        }

        logger.LogInformation("Email processor stopped.");
        await base.StopAsync(stoppingToken);
    }
    
    public async ValueTask DisposeAsync()
    {
        if (_processor is not null)
        {
            await _processor.DisposeAsync();
        }

        if (_client is not null)
        {
            await _client.DisposeAsync();
        }
    }
}