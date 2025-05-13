using Ardalis.Result;

namespace AktBob.FilArkiv.Contracts;

public interface IFilArkivModule
{
    Task<Result<IReadOnlyCollection<DocumentDto>>> GetDocumentsByCaseId(Guid caseId, CancellationToken cancellationToken = default);
    Task<Result<FileProcessStatusDto>> GetFileProcessStatus(Guid fileId,  CancellationToken cancellationToken = default);
}
