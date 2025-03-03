namespace AktBob.Podio.Contracts;
internal interface IUpdatePodioFieldHandler
{
    Task Handle(int appId, long itemId, int fieldId, string value, CancellationToken cancellationToken);
}