using AAK.GetOrganized.RelateDocuments;
using AAK.GetOrganized.UploadDocument;
using AktBob.GetOrganized.Contracts.DTOs;

namespace AktBob.GetOrganized.Contracts;

public interface IGetOrganizedModule
{
    void FinalizeDocument(int documentId);
    Task<Result<CreateCaseResponse>> CreateCase(string caseTypePrefix, string caseTitle, string description, string status, string access, CancellationToken cancellationToken);
    Task RelateDocuments(int parentDocumentId, int[] childDocumentIds, RelationType relationType = RelationType.Bilag, CancellationToken cancellationToken = default);
    Task<Result<int>> UploadDocument(byte[] bytes, string caseNumber, string fileName, UploadDocumentMetadata metadata, bool overwrite, CancellationToken cancellationToken);
}
