using Ardalis.Result;

namespace AktBob.FilArkiv.Contracts;
internal interface IGetDocumentsHandler
{
    Task<Result<IReadOnlyCollection<DocumentDto>>> Handle(Guid caseId, CancellationToken cancellationToken = default);
}
