namespace AktBob.UiPath.Contracts;

public interface ICreateUiPathQueueItemHandler
{
    Task Handle(string queueName, string reference, string payload, CancellationToken cancellationToken);
}