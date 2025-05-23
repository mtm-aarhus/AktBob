using AktBob.GetOrganized.Contracts;
using AktBob.GetOrganized.Contracts.DTOs;
using AktBob.GetOrganized.Jobs;
using AktBob.Shared;
using Ardalis.Result;
using ErrorOr;

namespace AktBob.GetOrganized;

internal class GetOrganizedModule(
    IJobDispatcher jobDispatcher,
    ICreateCaseHandler createCaseHandler,
    IRelateDocumentsHandler relateDocumentsHandler,
    IUploadDocumentHandler uploadDocumentHandler,
    IGetAggregatedCaseHandler aggregatedCaseHandler,
    IGetCaseMetadataHandler getCaseMetadataHandler,
    IUpdateCaseMetadata updateCaseMetadataHandler) : IGetOrganizedModule
{
    public async Task<Result<CreateCaseResponse>> CreateCase(CreateGetOrganizedCaseCommand command, CancellationToken cancellationToken)
        => await createCaseHandler.Handle(command, cancellationToken);

    public void FinalizeDocument(FinalizeDocumentCommand command) => jobDispatcher.Dispatch(new FinalizeDocumentJob(command.DocumentId, command.ShouldCloseOpenTasks));

    public async Task<IReadOnlyCollection<string>> GetAggregatedCase(string aggregatedCaseId, CancellationToken cancellationToken) => await aggregatedCaseHandler.Handle(aggregatedCaseId, cancellationToken);

    public async Task<ErrorOr<CaseMetadataDto>> GetCaseMetadata(string caseId, CancellationToken cancellationToken = default) => await getCaseMetadataHandler.Handle(caseId, cancellationToken);

    public async Task RelateDocuments(RelateDocumentsCommand command, CancellationToken cancellationToken = default) => await relateDocumentsHandler.Handle(command, cancellationToken);

    public Task<ErrorOr<Success>> UpdateCaseMetadata(string caseId, UpdateCaseMetadataCommand command, CancellationToken cancellationToken = default) => updateCaseMetadataHandler.Handle(caseId, command, cancellationToken);

    public async Task<Result<int>> UploadDocument(UploadDocumentCommand command, CancellationToken cancellationToken) => await uploadDocumentHandler.Handle(command, cancellationToken);

}
