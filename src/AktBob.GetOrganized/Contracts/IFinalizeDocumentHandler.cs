namespace AktBob.GetOrganized.Contracts;
internal interface IFinalizeDocumentHandler
{
    Task Handle(int DocumentId, bool ShouldCloseOpenTasks = false, CancellationToken cancellationToken = default);
}