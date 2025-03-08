namespace AktBob.GetOrganized.Contracts;
internal interface IFinalizeDocumentHandler
{
    Task Handle(FinalizeDocumentCommand command, CancellationToken cancellationToken = default);
}