using AAK.GetOrganized.RelateDocuments;

namespace AktBob.GetOrganized.Contracts;
internal interface IRelateDocumentsHandler
{
    Task Handle(int parentDocumentId, int[] childDocumentIds, RelationType relationType = RelationType.Bilag, CancellationToken cancellationToken = default);
}