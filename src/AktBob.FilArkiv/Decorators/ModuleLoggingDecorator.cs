using AktBob.FilArkiv.Contracts;
using Ardalis.Result;
using Microsoft.Extensions.Logging;

namespace AktBob.FilArkiv.Decorators;
internal class ModuleLoggingDecorator : IFilArkivModule
{
    private readonly IFilArkivModule _inner;
    private readonly ILogger<FilArkivModule> _logger;

    public ModuleLoggingDecorator(IFilArkivModule inner, ILogger<FilArkivModule> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyCollection<DocumentDto>>> GetDocumentsByCaseId(Guid caseId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting FilArkiv documents by case {caseId}", caseId);

        var result = await _inner.GetDocumentsByCaseId(caseId, cancellationToken);
        if (!result.IsSuccess)
        {
            _logger.LogWarning("{name}: {errors}", nameof(GetDocumentsByCaseId), result.Errors);
        }

        return result;
    }

    public async Task<Result<FileProcessStatusDto>> GetFileProcessStatus(Guid fileId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting file process status for file {fileId}", fileId);

        var result = await _inner.GetFileProcessStatus(fileId, cancellationToken);
        if (!result.IsSuccess)
        {
            _logger.LogWarning("{name}: {errors}", nameof(GetFileProcessStatus), result.Errors);
        }

        return result;
    }
}
