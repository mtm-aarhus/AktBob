using AktBob.Queue.Contracts;

namespace AktBob.Queue;
public interface IQueue
{
    //Task<string> CreateMessage(string connectionString, string queueName, string message, CancellationToken cancellationToken = default);
    Task DeleteMessage(string connectionString, string queueName, string messageId, string popReciept, CancellationToken cancellationToken = default);
    Task<IEnumerable<QueueMessageDto>> GetMessages(string connectionString, string queueName, int visibilyTimeoutSeconds = 60, int maxMessage = 10, CancellationToken cancellationToken = default);
}
