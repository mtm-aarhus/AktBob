namespace AktBob.Podio.Contracts;
internal interface IUpdateTextFieldHandler
{
    Task Handle(UpdateTextFieldCommand command, CancellationToken cancellationToken);
}