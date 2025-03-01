using Ardalis.Result;

namespace AktBob.OpenOrchestrator.Contracts;
public interface ICreateOpenOrchestratorQueueItemHandler
{
    Task<Result<Guid>> Handle(string queueName, string payload, string reference, CancellationToken cancellationToken);
}