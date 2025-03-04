using Ardalis.Result;

namespace AktBob.OpenOrchestrator;
internal interface ICreateQueueItemHandler
{
    Task<Result<Guid>> Handle(string queueName, string payload, string reference, CancellationToken cancellationToken);
}