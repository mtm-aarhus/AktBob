namespace AktBob.GetOrganized.Contracts;
internal interface IFinalizeDocumentHandler
{
    Task Handle(int documentId, bool shouldCloseOpenTasks = false, CancellationToken cancellationToken = default);
}