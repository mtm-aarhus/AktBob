namespace AktBob.CreateOCRScreeningStatus.ExternalQueue;
public interface ICheckOCRScreeningStatusQueue
{
    Task<string> CreateMessage(string message);
    Task DeleteMessage(string messageId, string popReciept, CancellationToken cancellationToken = default);
    Task<IEnumerable<QueueMessageDto>> GetMessages(int maxMessage = 10);
}
