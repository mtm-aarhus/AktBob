namespace AktBob.CreateOCRScreeningStatus.ExternalQueue;
public interface IQueue
{
    Task<string> CreateMessage(string message);
    Task DeleteMessage(string messageId, string popReciept, CancellationToken cancellationToken = default);
    Task<IEnumerable<QueueMessageDto>> GetMessages(int maxMessage = 10);
}
