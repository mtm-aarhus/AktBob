using AktBob.GetOrganized.Contracts;
using AktBob.GetOrganized.Contracts.DTOs;
using Ardalis.Result;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace AktBob.GetOrganized.Decorators;

internal class ModuleExceptionDecorator : IGetOrganizedModule
{
    private readonly IGetOrganizedModule _inner;
    private readonly ILogger<GetOrganizedModule> _logger;

    public ModuleExceptionDecorator(IGetOrganizedModule inner, ILogger<GetOrganizedModule> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<Result<CreateCaseResponse>> CreateCase(CreateGetOrganizedCaseCommand command, CancellationToken cancellationToken)
    {
        try
        {
            return await _inner.CreateCase(command, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(CreateCase));
            throw;
        }
    }

    public void FinalizeDocument(FinalizeDocumentCommand command)
    {
        try
        {
            _inner.FinalizeDocument(command);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(FinalizeDocument));
            throw;
        }
    }

    public async Task<IReadOnlyCollection<string>> GetAggregatedCase(string aggregatedCaseId, CancellationToken cancellationToken)
    {
        try
        {
            return await _inner.GetAggregatedCase(aggregatedCaseId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(GetAggregatedCase));
            throw;
        }
    }

    public async Task<ErrorOr<CaseMetadataDto>> GetCaseMetadata(string caseId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _inner.GetCaseMetadata(caseId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(GetCaseMetadata));
            throw;
        }
    }

    public async Task RelateDocuments(RelateDocumentsCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            await _inner.RelateDocuments(command, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(RelateDocuments));
            throw;
        }
    }

    public async Task<ErrorOr<Success>> UpdateCaseMetadata(string caseId, UpdateCaseMetadataCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _inner.UpdateCaseMetadata(caseId, command, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(UpdateCaseMetadata));
            throw;
        }
    }

    public async Task<Result<int>> UploadDocument(UploadDocumentCommand command, CancellationToken cancellationToken)
    {
        try
        {
            return await _inner.UploadDocument(command, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(UploadDocument));
            throw;
        }
    }
}
