namespace AktBob.UiPath.Contracts;

internal interface ICreateQueueItemHandler
{
    Task Handle(string queueName, string reference, string payload, CancellationToken cancellationToken);
}