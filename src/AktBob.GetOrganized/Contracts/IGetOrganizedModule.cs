using AktBob.GetOrganized.Contracts.DTOs;
using ErrorOr;

namespace AktBob.GetOrganized.Contracts;

public interface IGetOrganizedModule
{
    void FinalizeDocument(int documentId, bool shouldCloseOpenTasks);
    
    Task<ErrorOr<CreateCaseResponse>> CreateCase(string caseTitle,
        string caseProfile,
        string status,
        string access,
        string department,
        string facet,
        string kle,
        CancellationToken cancellationToken);
    
    Task RelateDocuments(int parentDocumentId, int[] childrenDocumentsIds, CancellationToken cancellationToken = default);
    Task<ErrorOr<int>> UploadDocument(
        byte[] bytes,
        string caseNumber,
        string fileName,
        string customProperty,
        DateTime documentDate,
        UploadDocumentCategory category,
        bool overwriteExisting,
        CancellationToken cancellationToken);
    Task<ErrorOr<IReadOnlyCollection<string>>> GetAggregatedCase(string aggregatedCaseId, CancellationToken cancellationToken);
    Task<ErrorOr<CaseMetadataDto>> GetCaseMetadata(string caseId, CancellationToken cancellationToken = default);
    Task<ErrorOr<Success>> UpdateCaseMetadata(string caseId, string kle, CancellationToken cancellationToken = default);
}
