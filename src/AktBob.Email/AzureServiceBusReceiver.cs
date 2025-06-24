using AktBob.Email.Handler;
using AktBob.Shared;
using AktBob.Shared.Contracts.Processors;
using Ardalis.GuardClauses;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AktBob.Email;

internal class AzureServiceBusReceiver(IConfiguration configuration, IServiceScopeFactory scopeFactory, ILogger<AzureServiceBusReceiver> logger) : BackgroundService
{
    private ServiceBusProcessor? _processor;
    private ServiceBusClient? _client;

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
        var connectionString = Guard.Against.NullOrEmpty(configuration.GetConnectionString("AzureServiceBus"));
        var queueName = Guard.Against.NullOrEmpty(configuration.GetValue<string>("EmailModule:ServiceBusQueueName"));
        
        _client = new ServiceBusClient(connectionString, _serviceBusClientOptions);
        _processor = _client.CreateProcessor(queueName, new ServiceBusProcessorOptions());
        
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