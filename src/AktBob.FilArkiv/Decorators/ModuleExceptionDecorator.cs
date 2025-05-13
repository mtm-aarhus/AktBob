using AktBob.FilArkiv.Contracts;
using Ardalis.Result;
using Microsoft.Extensions.Logging;

namespace AktBob.FilArkiv.Decorators;
internal class ModuleExceptionDecorator : IFilArkivModule
{
    private readonly IFilArkivModule _inner;
    private readonly ILogger<FilArkivModule> _logger;

    public ModuleExceptionDecorator(IFilArkivModule inner, ILogger<FilArkivModule> logger)
    {
        _inner = inner;
        _logger = logger;
    }
    public async Task<Result<IReadOnlyCollection<DocumentDto>>> GetDocumentsByCaseId(Guid caseId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _inner.GetDocumentsByCaseId(caseId, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Documents for FilArkiv case {caseId} not found.", caseId);
                return Result.NotFound();
            }

            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(GetDocumentsByCaseId));
            throw;
        }
    }

    public async Task<Result<FileProcessStatusDto>> GetFileProcessStatus(Guid fileId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _inner.GetFileProcessStatus(fileId, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("File process status for FilArkiv file {fileId} not found.", fileId);
                return Result.NotFound();
            }

            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(GetFileProcessStatus));
            throw;
        }
    }
}
