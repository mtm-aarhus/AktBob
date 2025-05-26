using AktBob.FilArkiv.Contracts.DTOs;
using ErrorOr;

namespace AktBob.FilArkiv.Handlers.GetDocumentsByCaseId;
internal interface IGetDocumentsByCaseIdHandler
{
    Task<ErrorOr<IReadOnlyCollection<DocumentDto>>> Handle(Guid caseId, CancellationToken cancellationToken = default);
}