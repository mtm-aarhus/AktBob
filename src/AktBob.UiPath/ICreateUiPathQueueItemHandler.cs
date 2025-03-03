namespace AktBob.UiPath.Contracts;

internal interface ICreateUiPathQueueItemHandler
{
    Task Handle(string queueName, string reference, string payload, CancellationToken cancellationToken);
}