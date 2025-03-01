namespace AktBob.Podio.Contracts;
public interface IUpdatePodioFieldHandler
{
    Task Handle(int appId, long itemId, int fieldId, string value, CancellationToken cancellationToken);
}