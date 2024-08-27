using AktBob.Queue.Contracts;

namespace AktBob.Queue;
public interface IQueue
{
    Task DeleteMessage(string queueName, string messageId, string popReciept, CancellationToken cancellationToken = default);
    Task<IEnumerable<QueueMessageDto>> GetMessages(string queueName, int visibilyTimeoutSeconds = 60, int maxMessage = 10, CancellationToken cancellationToken = default);
}
