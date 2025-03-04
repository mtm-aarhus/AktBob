namespace AktBob.UiPath.Contracts;

public interface IUiPathModule
{
    void CreateQueueItem(string queueName, string reference, string payload);
}
