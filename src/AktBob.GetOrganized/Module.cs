using AAK.GetOrganized.RelateDocuments;
using AAK.GetOrganized.UploadDocument;
using AktBob.GetOrganized.Contracts;
using AktBob.GetOrganized.Contracts.DTOs;
using AktBob.GetOrganized.Contracts.Jobs;
using AktBob.Shared;
using Ardalis.Result;

namespace AktBob.GetOrganized;

internal class Module(
    IJobDispatcher jobDispatcher,
    ICreateCaseHandler createCaseHandler,
    IRelateDocumentsHandler relateDocumentsHandler,
    IUploadDocumentHandler uploadDocumentHandler) : IGetOrganizedModule
{
    public async Task<Result<CreateCaseResponse>> CreateCase(string caseTypePrefix, string caseTitle, string description, string status, string access, CancellationToken cancellationToken)
        => await createCaseHandler.Handle(caseTypePrefix, caseTitle, description, status, access, cancellationToken);

    public void FinalizeDocument(int documentId) => jobDispatcher.Dispatch(new FinalizeDocumentJob(documentId));

    public async Task RelateDocuments(int parentDocumentId, int[] childDocumentIds, RelationType relationType = RelationType.Bilag, CancellationToken cancellationToken = default)
        => await relateDocumentsHandler.Handle(parentDocumentId, childDocumentIds, relationType, cancellationToken);

    public async Task<Result<int>> UploadDocument(byte[] bytes, string caseNumber, string fileName, UploadDocumentMetadata metadata, bool overwrite, CancellationToken cancellationToken)
        => await uploadDocumentHandler.Handle(bytes, caseNumber, fileName, metadata, overwrite, cancellationToken);
}
