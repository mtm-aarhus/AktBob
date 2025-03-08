namespace AktBob.GetOrganized.Contracts;

public record RelateDocumentsCommand(int ParentDocumentId, int[] ChildrenDocumentsIds);