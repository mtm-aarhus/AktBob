namespace AktBob.GetOrganized.Handlers.FinalizeDocument;
internal interface IFinalizeDocumentHandler
{
    Task Handle(int documentId, bool shouldCloseOpenTasks = false, CancellationToken cancellationToken = default);
}