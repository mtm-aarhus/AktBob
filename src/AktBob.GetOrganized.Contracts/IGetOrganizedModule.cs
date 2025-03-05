using AAK.GetOrganized.RelateDocuments;
using AAK.GetOrganized.UploadDocument;
using AktBob.GetOrganized.Contracts.DTOs;

namespace AktBob.GetOrganized.Contracts;

public interface IGetOrganizedModule
{
    void FinalizeDocument(int documentId);
    Task<Result<CreateCaseResponse>> CreateCase(string caseTitle, string caseProfile, string status, string access, string department, string facet, string kle, CancellationToken cancellationToken);
    Task RelateDocuments(int parentDocumentId, int[] childDocumentIds, RelationType relationType = RelationType.Bilag, CancellationToken cancellationToken = default);
    Task<Result<int>> UploadDocument(byte[] bytes, string caseNumber, string fileName, UploadDocumentMetadata metadata, bool overwrite, CancellationToken cancellationToken);
}
