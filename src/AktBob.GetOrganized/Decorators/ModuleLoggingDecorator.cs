using AktBob.GetOrganized.Contracts;
using AktBob.GetOrganized.Contracts.DTOs;
using Ardalis.Result;
using Microsoft.Extensions.Logging;

namespace AktBob.GetOrganized.Decorators;

internal class ModuleLoggingDecorator(IGetOrganizedModule inner, ILogger<GetOrganizedModule> logger) : IGetOrganizedModule
{
    private readonly IGetOrganizedModule _inner = inner;
    private readonly ILogger<GetOrganizedModule> _logger = logger;

    public async Task<Result<CreateCaseResponse>> CreateCase(CreateGetOrganizedCaseCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating GetOrganized case: {command}", command);

        var result = await _inner.CreateCase(command, cancellationToken);
        if (!result.IsSuccess)
        {
            _logger.LogDebug("{name}: {errors}", nameof(CreateCase), result.Errors);
        }

        _logger.LogInformation("GetOrganized case created. Command: {command} Response: {response}", command, result.Value);
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
            _logger.LogDebug("{name}: Error getting case numbers from aggregated GetOrganized case {id}", nameof(GetAggregatedCase), aggregatedCaseId);
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
            _logger.LogDebug("{name}: {errors}", nameof(UploadDocument), result.Errors);
        }

        return result;
    }
}
