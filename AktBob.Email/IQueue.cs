namespace AktBob.Email;
public interface IQueue
{
    Task<string> QueueMessage(string message);
    Task DeleteMessage(string messageId, string popReciept, CancellationToken cancellationToken = default);
    Task<IEnumerable<QueueMessageDto>> GetMessages(int maxMessage = 10);
}
