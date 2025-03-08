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

    public void FinalizeDocument(FinalizeDocumentCommand command) => jobDispatcher.Dispatch(new FinalizeDocumentJob(command));

    public async Task<IReadOnlyCollection<string>> GetAggregatedCase(string aggregatedCaseId, CancellationToken cancellationToken) => await aggregatedCaseHandler.Handle(aggregatedCaseId, cancellationToken);

    public async Task RelateDocuments(RelateDocumentsCommand command, CancellationToken cancellationToken = default)
        => await relateDocumentsHandler.Handle(command, cancellationToken);

    public async Task<Result<int>> UploadDocument(UploadDocumentCommand command, CancellationToken cancellationToken)
        => await uploadDocumentHandler.Handle(command, cancellationToken);

}
