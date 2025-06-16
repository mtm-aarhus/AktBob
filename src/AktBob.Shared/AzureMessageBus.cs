using System.Text.Json;
using Ardalis.GuardClauses;
using Azure.Messaging.ServiceBus;
using ErrorOr;
using Microsoft.Extensions.Configuration;

namespace AktBob.Shared;

internal class AzureMessageBus(IConfiguration configuration) : IMessageBus
{
    private readonly string _connectionString = Guard.Against.NullOrEmpty(configuration.GetConnectionString("AzureServiceBus"));
    private readonly ServiceBusClientOptions _options = new() { TransportType = ServiceBusTransportType.AmqpWebSockets };
    
    public async Task<ErrorOr<Success>> SendMessage(string queue, object? payload, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var client = new ServiceBusClient(_connectionString, _options);
            await using var sender = client.CreateSender(queue);

            var payloadSerialized = payload is null ? string.Empty : JsonSerializer.Serialize(payload, SerializerConfiguration.SerializerOptions());
            var message = new ServiceBusMessage(payloadSerialized)
            {
                ContentType = "application/json"
            };

            await sender.SendMessageAsync(message, cancellationToken);

            return Result.Success;
        }
        catch (Exception e)
        {
            return Error.Failure("AzureMessageBus.SendMessage", $"Error: {e.Message} StackTrace: {e.StackTrace}");
        }
    }

    public async Task<ErrorOr<Success>> ScheduleMessage(string queue, object? payload, DateTimeOffset offset, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var client = new ServiceBusClient(_connectionString, _options);
            await using var sender = client.CreateSender(queue);

            var payloadSerialized = payload is null ? string.Empty : JsonSerializer.Serialize(payload, SerializerConfiguration.SerializerOptions());
            var message = new ServiceBusMessage(payloadSerialized)
            {
                ContentType = "application/json"
            };

            await sender.ScheduleMessageAsync(message, offset, cancellationToken);

            return Result.Success;
        }
        catch (Exception e)
        {
            return Error.Failure("AzureMessageBus.ScheduleMessage", $"Error: {e.Message} StackTrace: {e.StackTrace}");
        }
    }
}