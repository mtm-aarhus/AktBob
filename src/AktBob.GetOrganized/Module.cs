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
    IUploadDocumentHandler uploadDocumentHandler) : IGetOrganizedModule
{
    public async Task<Result<CreateCaseResponse>> CreateCase(string caseTitle, string caseProfile, string status, string access, string department, string facet, string kle, CancellationToken cancellationToken)
        => await createCaseHandler.Handle(caseTitle, caseProfile, status, access, department, facet, kle, cancellationToken);

    public void FinalizeDocument(int documentId) => jobDispatcher.Dispatch(new FinalizeDocumentJob(documentId));

    public async Task RelateDocuments(int parentDocumentId, int[] childDocumentIds, RelationType relationType = RelationType.Bilag, CancellationToken cancellationToken = default)
        => await relateDocumentsHandler.Handle(parentDocumentId, childDocumentIds, relationType, cancellationToken);

    public async Task<Result<int>> UploadDocument(byte[] bytes, string caseNumber, string fileName, UploadDocumentMetadata metadata, bool overwrite, CancellationToken cancellationToken)
        => await uploadDocumentHandler.Handle(bytes, caseNumber, fileName, metadata, overwrite, cancellationToken);
}
