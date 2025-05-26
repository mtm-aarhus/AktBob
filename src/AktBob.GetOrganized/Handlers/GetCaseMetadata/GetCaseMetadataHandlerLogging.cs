using AktBob.GetOrganized.Contracts.DTOs;
using AktBob.Shared.Extensions;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace AktBob.GetOrganized.Handlers.GetCaseMetadata;

internal class GetCaseMetadataHandlerLogging : IGetCaseMetadataHandler
{
    private readonly IGetCaseMetadataHandler _next;
    private readonly ILogger<GetCaseMetadataHandler> _logger;

    public GetCaseMetadataHandlerLogging(IGetCaseMetadataHandler next, ILogger<GetCaseMetadataHandler> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task<ErrorOr<CaseMetadataDto>> Handle(string caseId, CancellationToken cancellation = default)
    {
        _logger.LogInformation("Getting GetOrganized case {id} metadata", caseId);

        var result = await _next.Handle(caseId, cancellation);

        result.Switch(
            _ => _logger.LogInformation("GetOrganized case {id} metadata retrieved", caseId),
            errors => _logger.LogWarning("Error getting GetOrganized case {id} metadata: {errors}", caseId, errors.ToCommaDelimitedString()));

        return result;
    }
}