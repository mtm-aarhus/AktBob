using AAK.GetOrganized.RelateDocuments;
using AAK.GetOrganized.UploadDocument;
using AktBob.GetOrganized.Contracts;
using AktBob.GetOrganized.Contracts.DTOs;
using AktBob.GetOrganized.Jobs;
using AktBob.Shared;
using Ardalis.Result;

namespace AktBob.GetOrganized;

internal class Module(
    IJobDispatcher jobDispatcher,
    ICreateCaseHandler createCaseHandler,
    IRelateDocumentsHandler relateDocumentsHandler,
    IUploadDocumentHandler uploadDocumentHandler,
    IGetAggregatedCaseHandler aggregatedCaseHandler) : IGetOrganizedModule
{
    public async Task<Result<CreateCaseResponse>> CreateCase(CreateGetOrganizedCaseCommand command, CancellationToken cancellationToken)
        => await createCaseHandler.Handle(command, cancellationToken);

    public void FinalizeDocument(int documentId) => jobDispatcher.Dispatch(new FinalizeDocumentJob(documentId));

    public async Task<IReadOnlyCollection<string>> GetAggregatedCase(string aggregatedCaseId, CancellationToken cancellationToken) => await aggregatedCaseHandler.Handle(aggregatedCaseId, cancellationToken);

    public async Task RelateDocuments(int parentDocumentId, int[] childDocumentIds, RelationType relationType = RelationType.Bilag, CancellationToken cancellationToken = default)
        => await relateDocumentsHandler.Handle(parentDocumentId, childDocumentIds, relationType, cancellationToken);

    public async Task<Result<int>> UploadDocument(byte[] bytes, string caseNumber, string fileName, UploadDocumentMetadata metadata, bool overwrite, CancellationToken cancellationToken)
        => await uploadDocumentHandler.Handle(bytes, caseNumber, fileName, metadata, overwrite, cancellationToken);
}
