namespace AktBob.Podio.Contracts;
internal interface IUpdateTextFieldHandler
{
    Task Handle(int appId, long itemId, int fieldId, string value, CancellationToken cancellationToken);
}