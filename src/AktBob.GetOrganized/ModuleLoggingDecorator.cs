using AktBob.GetOrganized.Contracts;
using AktBob.GetOrganized.Contracts.DTOs;
using Ardalis.Result;
using Microsoft.Extensions.Logging;

namespace AktBob.GetOrganized;

internal class ModuleLoggingDecorator(IGetOrganizedModule inner, ILogger<ModuleLoggingDecorator> logger) : IGetOrganizedModule
{
    private readonly IGetOrganizedModule _inner = inner;
    private readonly ILogger<ModuleLoggingDecorator> _logger = logger;

    public async Task<Result<CreateCaseResponse>> CreateCase(CreateGetOrganizedCaseCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating GetOrganized case: {command}", command);

        var result = await _inner.CreateCase(command, cancellationToken);
        if (!result.IsSuccess)
        {
            _logger.LogWarning("GetOrganized case not created. Parameters: {command}, Errors: {errors}", command, result.Errors);
        }

        return result;
    }

    public void FinalizeDocument(FinalizeDocumentCommand command)
    {
        _logger.LogInformation("Enqueuing job: Finalize GetOrganized document {command}", command);
        _inner.FinalizeDocument(command);
    }

    public async Task<IReadOnlyCollection<string>> GetAggregatedCase(string aggregatedCaseId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting GetOrganized case numbers from aggregated case {caseId}", aggregatedCaseId);

        var result = await _inner.GetAggregatedCase(aggregatedCaseId, cancellationToken);
        
        if (result is null)
        {
            _logger.LogWarning("Error getting case numbers from aggregated GetOrganized case {id}", aggregatedCaseId);
        }

        if (result is not null)
        {
            _logger.LogInformation("{count} cases found in aggregated GetOrganized case {id}", result.Count, aggregatedCaseId);
        }

        return result!;
    }

    public async Task RelateDocuments(RelateDocumentsCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Relating GetOrganized documents. {command}", command);
        await _inner.RelateDocuments(command, cancellationToken);
    }

    public async Task<Result<int>> UploadDocument(UploadDocumentCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Uploading document to GetOrganized case. CaseId = {caseId}, Filename = {filename}", command.CaseNumber, command.FileName);

        var result = await _inner.UploadDocument(command, cancellationToken);
        if (!result.IsSuccess)
        {
            _logger.LogWarning(string.Join(",", result.Errors));
        }

        return result;
    }
}
