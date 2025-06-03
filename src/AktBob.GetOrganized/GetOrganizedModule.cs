using AktBob.GetOrganized.Contracts;
using AktBob.GetOrganized.Contracts.DTOs;
using AktBob.GetOrganized.Handlers.CreateCase;
using AktBob.GetOrganized.Handlers.GetAggregatedCase;
using AktBob.GetOrganized.Handlers.GetCaseMetadata;
using AktBob.GetOrganized.Handlers.RelateDocuments;
using AktBob.GetOrganized.Handlers.UpdateCaseMetadata;
using AktBob.GetOrganized.Handlers.UploadDocument;
using AktBob.GetOrganized.Jobs;
using AktBob.Shared;
using ErrorOr;

namespace AktBob.GetOrganized;

internal class GetOrganizedModule(
    IJobDispatcher jobDispatcher,
    ICreateCaseHandler createCaseHandler,
    IRelateDocumentsHandler relateDocumentsHandler,
    IUploadDocumentHandler uploadDocumentHandler,
    IGetAggregatedCaseHandler aggregatedCaseHandler,
    IGetCaseMetadataHandler getCaseMetadataHandler,
    IUpdateCaseMetadataHandler updateCaseMetadataHandlerHandler) : IGetOrganizedModule
{
    public async Task<ErrorOr<CreateCaseResponse>> CreateCase(
        string caseTitle,
        string caseProfile,
        string status,
        string access,
        string department,
        string facet,
        string kle,
        CancellationToken cancellationToken) => await createCaseHandler.Handle(caseTitle, caseProfile, status, access, department, facet, kle, cancellationToken);

    public void FinalizeDocument(int documentId, bool shouldCloseOpenTasks) => jobDispatcher.Dispatch(new FinalizeDocumentJob(documentId, shouldCloseOpenTasks));

    public async Task<ErrorOr<IReadOnlyCollection<string>>> GetAggregatedCase(string aggregatedCaseId, CancellationToken cancellationToken) => await aggregatedCaseHandler.Handle(aggregatedCaseId, cancellationToken);

    public async Task<ErrorOr<CaseMetadataDto>> GetCaseMetadata(string caseId, CancellationToken cancellationToken = default) => await getCaseMetadataHandler.Handle(caseId, cancellationToken);

    public async Task RelateDocuments(int parentDocumentId, int[] childrenDocumentsIds, CancellationToken cancellationToken = default) => await relateDocumentsHandler.Handle(parentDocumentId, childrenDocumentsIds, cancellationToken);

    public Task<ErrorOr<Success>> UpdateCaseMetadata(string caseId, Guid kleId, CancellationToken cancellationToken = default) => updateCaseMetadataHandlerHandler.Handle(caseId, kleId, cancellationToken);

    public async Task<ErrorOr<int>> UploadDocument(
        byte[] bytes,
        string caseNumber,
        string fileName,
        string customProperty,
        DateTime documentDate,
        UploadDocumentCategory category,
        bool overwriteExisting,
        CancellationToken cancellationToken) 
        => await uploadDocumentHandler.Handle(
            bytes,
            caseNumber,
            fileName,
            customProperty,
            documentDate,
            category,
            overwriteExisting,
            cancellationToken);

}
