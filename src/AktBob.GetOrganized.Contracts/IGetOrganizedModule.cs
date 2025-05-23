using AktBob.GetOrganized.Contracts.DTOs;
using ErrorOr;

namespace AktBob.GetOrganized.Contracts;

public interface IGetOrganizedModule
{
    void FinalizeDocument(FinalizeDocumentCommand command);
    Task<Result<CreateCaseResponse>> CreateCase(CreateGetOrganizedCaseCommand command, CancellationToken cancellationToken);
    Task RelateDocuments(RelateDocumentsCommand command, CancellationToken cancellationToken = default);
    Task<Result<int>> UploadDocument(UploadDocumentCommand command, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<string>> GetAggregatedCase(string aggregatedCaseId, CancellationToken cancellationToken);
    Task<ErrorOr<CaseMetadataDto>> GetCaseMetadata(string caseId, CancellationToken cancellationToken = default);
    Task<ErrorOr<Success>> UpdateCaseMetadata(string caseId, UpdateCaseMetadataCommand command, CancellationToken cancellationToken = default);
}
