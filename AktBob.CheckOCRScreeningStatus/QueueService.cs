using AktBob.CreateOCRScreeningStatus.ExternalQueue;

namespace AktBob.CheckOCRScreeningStatus;
internal class QueueService : IQueueService
{
    public QueueService(string connectionString, string queueName, int visibilyTimeoutSeconds)
    {

        Queue = new Queue(connectionString, queueName, visibilyTimeoutSeconds);
    }

    public IQueue Queue { get; }
}
