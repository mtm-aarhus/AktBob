using System.Collections.ObjectModel;
using System.Text.Json;
using AktBob.Shared.Extensions;
using Ardalis.GuardClauses;
using Azure.Messaging.ServiceBus;
using ErrorOr;
using Microsoft.Extensions.Configuration;

namespace AktBob.Shared;

internal class AzureMessageBus(IConfiguration configuration) : IMessageBus
{
    private readonly string _connectionString = Guard.Against.NullOrEmpty(configuration.GetConnectionString("AzureServiceBus"));
    private readonly ServiceBusClientOptions _options = new() { TransportType = ServiceBusTransportType.AmqpWebSockets };

    private static ServiceBusMessage PayloadToMessage(object? payload)
    {
        var payloadSerialized = payload is null ? string.Empty : JsonSerializer.Serialize(payload, SerializerConfiguration.SerializerOptions());
        var message = new ServiceBusMessage(payloadSerialized)
        {
            ContentType = "application/json"
        };

        return message;
    }

    
    public async Task<ErrorOr<Success>> SendMessage(string queue, object? payload, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var client = new ServiceBusClient(_connectionString, _options);
            await using var sender = client.CreateSender(queue);
            var message = PayloadToMessage(payload);
            await sender.SendMessageAsync(message, cancellationToken);
            return Result.Success;
        }
        catch (Exception e)
        {
            return Error.Failure("AzureMessageBus.SendMessage", $"Error: {e.Message} StackTrace: {e.StackTrace}");
        }
    }

    public async Task<ErrorOr<Success>> SendMessages(string queue, object[]? payloads, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var client = new ServiceBusClient(_connectionString, _options);
            await using var sender = client.CreateSender(queue);
            var messages = new Collection<ServiceBusMessage>();
            messages.AddRange(payloads?.Select(PayloadToMessage) ?? []);
            await sender.SendMessagesAsync(messages, cancellationToken);
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
            var message = PayloadToMessage(payload);
            await sender.ScheduleMessageAsync(message, offset, cancellationToken);

            return Result.Success;
        }
        catch (Exception e)
        {
            return Error.Failure("AzureMessageBus.ScheduleMessage", $"Error: {e.Message} StackTrace: {e.StackTrace}");
        }
    }
}