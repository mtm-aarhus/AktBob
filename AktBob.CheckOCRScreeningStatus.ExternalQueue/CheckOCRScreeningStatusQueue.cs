using Ardalis.GuardClauses;
using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;

namespace AktBob.CreateOCRScreeningStatus.ExternalQueue;
public class CheckOCRScreeningStatusQueue : ICheckOCRScreeningStatusQueue
{
    private QueueClient _queue;
    private readonly IConfiguration _configuration;

    public CheckOCRScreeningStatusQueue(IConfiguration configuration)
    {
        var connectionString = Guard.Against.NullOrEmpty(configuration.GetConnectionString("AzureStorage"));
        var queueName = Guard.Against.NullOrEmpty(configuration.GetValue<string>("AzureQueue:CheckOCRScreeningStatus:QueueName"));

        _queue = new QueueClient(connectionString, queueName);
        _configuration = configuration;
    }

    public async Task<string> CreateMessage(string message)
    {
        var reciept = await _queue.SendMessageAsync(message);
        return reciept.Value.MessageId;
    }

    public async Task<IEnumerable<QueueMessageDto>> GetMessages(int maxMessage = 10)
    {
        var visibilityTimeout = _configuration.GetValue<int?>("AzureQueue:CheckOCRScreeningStatus:VisibilityTimeoutSeconds") ?? 600;

        var response = await _queue.ReceiveMessagesAsync(maxMessage, TimeSpan.FromSeconds(visibilityTimeout));
        var messages = response.Value;

        var dto = messages.Select(m => new QueueMessageDto(
            m.MessageId,
            m.Body.ToString(),
            m.PopReceipt));

        return dto;
    }

    public async Task DeleteMessage(string messageId, string popReciept, CancellationToken cancellationToken = default)
    {
        await _queue.DeleteMessageAsync(messageId, popReciept, cancellationToken);
    }
}
