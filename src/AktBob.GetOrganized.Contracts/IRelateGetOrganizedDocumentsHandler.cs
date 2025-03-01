using AAK.GetOrganized.RelateDocuments;

namespace AktBob.GetOrganized.Contracts;
public interface IRelateGetOrganizedDocumentsHandler
{
    Task Handle(int parentDocumentId, int[] childDocumentIds, RelationType relationType = RelationType.Bilag, CancellationToken cancellationToken = default);
}