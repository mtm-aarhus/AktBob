using AAK.GetOrganized.RelateDocuments;

namespace AktBob.GetOrganized.Contracts;
public record RelateDocumentCommand(int ParentDocumentId, int[] ChildDocumentIds, RelationType RelationType = RelationType.Bilag) : IRequest;
