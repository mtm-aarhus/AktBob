using AktBob.Shared.Contracts.Modules.FilArkiv.Contracts.DTOs;
using ErrorOr;

namespace Aktbob.Modules.FilArkiv.Features.GetDocumentsByCaseId;
internal interface IGetDocumentsByCaseIdHandler
{
    Task<ErrorOr<IReadOnlyCollection<DocumentDto>>> Handle(Guid caseId, CancellationToken cancellationToken = default);
}