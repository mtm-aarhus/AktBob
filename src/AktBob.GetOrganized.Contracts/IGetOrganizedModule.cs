using AktBob.GetOrganized.Contracts.DTOs;

namespace AktBob.GetOrganized.Contracts;

public interface IGetOrganizedModule
{
    void FinalizeDocument(FinalizeDocumentCommand command);
    Task<Result<CreateCaseResponse>> CreateCase(CreateGetOrganizedCaseCommand command, CancellationToken cancellationToken);
    Task RelateDocuments(RelateDocumentsCommand command, CancellationToken cancellationToken = default);
    Task<Result<int>> UploadDocument(UploadDocumentCommand command, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<string>> GetAggregatedCase(string aggregatedCaseId, CancellationToken cancellationToken);
}
