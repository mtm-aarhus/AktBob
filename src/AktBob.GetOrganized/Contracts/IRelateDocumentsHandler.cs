namespace AktBob.GetOrganized.Contracts;
internal interface IRelateDocumentsHandler
{
    Task Handle(RelateDocumentsCommand command, CancellationToken cancellationToken = default);
}