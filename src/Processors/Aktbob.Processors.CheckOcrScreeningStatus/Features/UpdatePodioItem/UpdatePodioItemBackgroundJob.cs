using AktBob.Shared;
using Ardalis.GuardClauses;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Aktbob.Processors.CheckOcrScreeningStatus.Features.UpdatePodioItem;

internal class UpdatePodioItemBackgroundJob(
    ILogger<UpdatePodioItemBackgroundJob> logger,
    IConfiguration configuration,
    IServiceScopeFactory scopeFactory) : BackgroundService
{
    private ServiceBusProcessor? _processor;
    private ServiceBusClient? _client;
    private readonly string _connectionString = Guard.Against.NullOrEmpty(configuration.GetConnectionString("AzureServiceBus"));
    private readonly string _queueName = Guard.Against.NullOrEmpty(configuration.GetValue<string>("CheckOcrScreeningStatus:ServiceBusQueueNames:UpdatePodioItem"));

    private readonly ServiceBusClientOptions _serviceBusClientOptions = new()
    {
        TransportType = ServiceBusTransportType.AmqpWebSockets
    };
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _client = new ServiceBusClient(_connectionString, _serviceBusClientOptions);
        _processor = _client.CreateProcessor(_queueName, new ServiceBusProcessorOptions());
        
        _processor.ProcessMessageAsync += MessageHandler; // Adds handler to process messages
        _processor.ProcessErrorAsync += ErrorHandler; // Adds handler to process any errors

        await _processor.StartProcessingAsync(stoppingToken);
    }
    
    private async Task MessageHandler(ProcessMessageEventArgs args)
    {
        try
        {
            using var scope =  scopeFactory.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<UpdatePodioItemHandler>();
            logger.LogInformation("Received: {body}", args.Message.Body.ToString());
        
            var job = MessageDeserializer.Deserialize<UpdatePodioItemJob>(args.Message);
            var result = await handler.Run(job, args.CancellationToken);
            if (result.IsError)
            {
                await args.DeadLetterMessageAsync(args.Message, deadLetterReason: result.FirstError.Code, deadLetterErrorDescription: result.FirstError.Description, cancellationToken: args.CancellationToken);
                return;
            }
        
            await args.CompleteMessageAsync(args.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occured while processing message");
            await args.DeadLetterMessageAsync(args.Message, deadLetterReason: ex.Message, deadLetterErrorDescription: ex.StackTrace, cancellationToken: args.CancellationToken);
        }
    }

    private Task ErrorHandler(ProcessErrorEventArgs args)
    {
        logger.LogError("Error handling {namespace}.{job}: {error}", nameof(CheckOcrScreeningStatus), nameof(UpdatePodioItemBackgroundJob), args.Exception.ToString());
        return Task.CompletedTask;
    }
    
    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Stopping {namespace}.{job} processor...", nameof(CheckOcrScreeningStatus), nameof(UpdatePodioItemBackgroundJob));

        if (_processor is not null)
        {
            try
            {
                await _processor.StopProcessingAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to stop the {namespace}.{job} processor cleanly.", nameof(CheckOcrScreeningStatus), nameof(UpdatePodioItemBackgroundJob));
            }
        }

        logger.LogInformation("{namespace}.{job} stopped.", nameof(CheckOcrScreeningStatus), nameof(UpdatePodioItemBackgroundJob));
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


