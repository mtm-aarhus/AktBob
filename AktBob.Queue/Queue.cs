using AktBob.Queue.Contracts;
using Ardalis.GuardClauses;
using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;

namespace AktBob.Queue;
public class Queue : IQueue
{
    private readonly IConfiguration _configuration;
    private readonly string _connectionString;

    public Queue(IConfiguration configuration)
    {
        _configuration = configuration;
        _connectionString = Guard.Against.NullOrEmpty(_configuration.GetConnectionString("AzureStorage"));

    }

    public async Task<IEnumerable<QueueMessageDto>> GetMessages(string queueName, int visibilyTimeoutSeconds = 60, int maxMessage = 10, CancellationToken cancellationToken = default)
    {
        var queue = new QueueClient(_connectionString, queueName);

        var response = await queue.ReceiveMessagesAsync(maxMessage, TimeSpan.FromSeconds(visibilyTimeoutSeconds), cancellationToken);
        var messages = response.Value;

        var dto = messages.Select(m => new QueueMessageDto(
            m.MessageId,
            m.Body.ToString(),
            m.PopReceipt));

        return dto;
    }

    public async Task DeleteMessage(string queueName, string messageId, string popReciept, CancellationToken cancellationToken = default)
    {
        var queue = new QueueClient(_connectionString, queueName);
        await queue.DeleteMessageAsync(messageId, popReciept, cancellationToken);
    }
}
