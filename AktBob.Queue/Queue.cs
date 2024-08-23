using AktBob.Queue.Contracts;
using Azure.Storage.Queues;

namespace AktBob.Queue;
public class Queue : IQueue
{
    private readonly int _visibilyTimeoutSeconds;

    //public async Task<string> CreateMessage(string connectionString, string queueName, string message, CancellationToken cancellationToken = default)
    //{
    //    var queue = new QueueClient(connectionString, queueName);
    //    var reciept = await queue.SendMessageAsync(message, cancellationToken);
    //    return reciept.Value.MessageId;
    //}

    public async Task<IEnumerable<QueueMessageDto>> GetMessages(string connectionString, string queueName, int visibilyTimeoutSeconds = 60, int maxMessage = 10, CancellationToken cancellationToken = default)
    {
        var queue = new QueueClient(connectionString, queueName);

        var response = await queue.ReceiveMessagesAsync(maxMessage, TimeSpan.FromSeconds(visibilyTimeoutSeconds), cancellationToken);
        var messages = response.Value;

        var dto = messages.Select(m => new QueueMessageDto(
            m.MessageId,
            m.Body.ToString(),
            m.PopReceipt));

        return dto;
    }

    public async Task DeleteMessage(string connectionString, string queueName, string messageId, string popReciept, CancellationToken cancellationToken = default)
    {
        if (connectionString is null)
        {
            throw new ArgumentNullException(nameof(connectionString));
        }

        var queue = new QueueClient(connectionString, queueName);
        await queue.DeleteMessageAsync(messageId, popReciept, cancellationToken);
    }
}
