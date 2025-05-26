using AktBob.FilArkiv.Contracts.DTOs;
using ErrorOr;

namespace AktBob.FilArkiv.Contracts;

public interface IFilArkivModule
{
    Task<ErrorOr<IReadOnlyCollection<DocumentDto>>> GetDocumentsByCaseId(Guid caseId, CancellationToken cancellationToken = default);
    Task<ErrorOr<FileProcessStatusDto>> GetFileProcessStatus(Guid fileId,  CancellationToken cancellationToken = default);
}
