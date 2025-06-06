using System.Text.Encodings.Web;
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
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
    
    public async Task<ErrorOr<Success>> SendMessage(string queue, object? payload, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var client = new ServiceBusClient(_connectionString, _options);
            await using var sender = client.CreateSender(queue);

            var payloadSerialized = payload is null ? string.Empty : JsonSerializer.Serialize(payload, _jsonSerializerOptions);
            var message = new ServiceBusMessage(payloadSerialized);
            await sender.SendMessageAsync(message, cancellationToken);

            return Result.Success;
        }
        catch (Exception e)
        {
            return Error.Failure("AzureMessageBus.SendMessage", $"Error: {e.Message} StackTrace: {e.StackTrace}");
        }
    }
}