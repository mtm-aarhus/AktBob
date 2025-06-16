using System.Text.Json;
using AktBob.Shared.Exceptions;
using AktBob.Shared.Processors;
using Azure.Messaging.ServiceBus;

namespace AktBob.Shared;

public static class MessageDeserializer
{
    public static T Deserialize<T>(ServiceBusReceivedMessage message)
    {
        var job = JsonSerializer.Deserialize<T>(message.Body, SerializerConfiguration.SerializerOptions());
        if (job is null) throw new BusinessException($"{LogSnippets.MessageDeliveryCount(message.MessageId, message.DeliveryCount)}: Message body could not be deserialized to type {typeof(T).Name}. Body content = {message.Body}");
        return job;
    }
}