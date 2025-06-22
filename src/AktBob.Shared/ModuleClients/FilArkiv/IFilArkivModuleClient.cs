using AktBob.Shared.Contracts.Modules.FilArkiv.Contracts.DTOs;
using ErrorOr;

namespace AktBob.Shared.ModuleClients.FilArkiv;

public interface IFilArkivModuleClient
{
    Task<ErrorOr<IReadOnlyCollection<DocumentDto>>> GetDocumentsByCaseId(Guid caseId, CancellationToken cancellationToken = default);
    Task<ErrorOr<FileProcessStatusDto>> GetFileProcessStatus(Guid fileId, CancellationToken cancellationToken = default);
}