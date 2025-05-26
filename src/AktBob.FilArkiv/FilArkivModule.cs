using AktBob.FilArkiv.Contracts;
using AktBob.FilArkiv.Contracts.DTOs;
using AktBob.FilArkiv.Handlers.GetDocumentsByCaseId;
using AktBob.FilArkiv.Handlers.GetFileProcessStatus;
using ErrorOr;

namespace AktBob.FilArkiv;
internal class FilArkivModule : IFilArkivModule
{
    private readonly IGetDocumentsByCaseIdHandler _getDocumentsByCaseIdHandler;
    private readonly IGetFileProcessStatusHandler _getFileProcessStatusHandler;

    public FilArkivModule(
        IGetDocumentsByCaseIdHandler getDocumentsByCaseIdHandler,
        IGetFileProcessStatusHandler getFileProcessStatusHandler)
    {
        _getDocumentsByCaseIdHandler = getDocumentsByCaseIdHandler;
        _getFileProcessStatusHandler = getFileProcessStatusHandler;
    }

    public async Task<ErrorOr<IReadOnlyCollection<DocumentDto>>> GetDocumentsByCaseId(Guid caseId, CancellationToken cancellationToken = default) => await _getDocumentsByCaseIdHandler.Handle(caseId, cancellationToken);

    public async Task<ErrorOr<FileProcessStatusDto>> GetFileProcessStatus(Guid fileId, CancellationToken cancellationToken = default) => await _getFileProcessStatusHandler.Handle(fileId, cancellationToken);
}
