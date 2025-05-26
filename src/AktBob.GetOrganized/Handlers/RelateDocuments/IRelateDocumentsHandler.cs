namespace AktBob.GetOrganized.Handlers.RelateDocuments;
internal interface IRelateDocumentsHandler
{
    Task Handle(int parentDocumentId, int[] childrenDocumentsIds, CancellationToken cancellationToken = default);
}