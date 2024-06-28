using Ardalis.GuardClauses;
using Azure.Storage.Queues;

namespace AktBob.CreateOCRScreeningStatus.ExternalQueue;
public class Queue : IQueue
{
    private QueueClient _queue;
    private readonly int _visibilyTimeoutSeconds;

    public Queue(string connectionString, string queueName, int visibilyTimeoutSeconds)
    {
        Guard.Against.NullOrEmpty(connectionString);
        Guard.Against.NullOrEmpty(queueName);

        _queue = new QueueClient(connectionString, queueName);
        _visibilyTimeoutSeconds = visibilyTimeoutSeconds;
    }

    public async Task<string> CreateMessage(string message)
    {
        var reciept = await _queue.SendMessageAsync(message);
        return reciept.Value.MessageId;
    }

    public async Task<IEnumerable<QueueMessageDto>> GetMessages(int maxMessage = 10)
    {
        var response = await _queue.ReceiveMessagesAsync(maxMessage, TimeSpan.FromSeconds(_visibilyTimeoutSeconds));
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
