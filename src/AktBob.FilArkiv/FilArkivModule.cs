using AktBob.FilArkiv.Contracts;
using Ardalis.Result;

namespace AktBob.FilArkiv;
internal class FilArkivModule : IFilArkivModule
{
    private readonly IGetDocumentsHandler _getDocumentsHandler;
    private readonly IGetFileProcessStatusHandler _getFileProcessStatusHandler;

    public FilArkivModule(
        IGetDocumentsHandler getDocumentsHandler,
        IGetFileProcessStatusHandler getFileProcessStatusHandler)
    {
        _getDocumentsHandler = getDocumentsHandler;
        _getFileProcessStatusHandler = getFileProcessStatusHandler;
    }

    public async Task<Result<IReadOnlyCollection<DocumentDto>>> GetDocumentsByCaseId(Guid caseId, CancellationToken cancellationToken = default) => await _getDocumentsHandler.Handle(caseId, cancellationToken);

    public async Task<Result<FileProcessStatusDto>> GetFileProcessStatus(Guid fileId, CancellationToken cancellationToken = default) => await _getFileProcessStatusHandler.Handle(fileId, cancellationToken);
}
