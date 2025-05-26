using AktBob.Shared.Extensions;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace  AktBob.GetOrganized.Handlers.GetAggregatedCase;

internal class GetAggregatedCaseHandlerLogging : IGetAggregatedCaseHandler
{
    private readonly IGetAggregatedCaseHandler _next;
    private readonly ILogger<GetAggregatedCaseHandler> _logger;

    public GetAggregatedCaseHandlerLogging(IGetAggregatedCaseHandler next, ILogger<GetAggregatedCaseHandler> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task<ErrorOr<IReadOnlyCollection<string>>> Handle(string aggregatedCaseId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting GetOrganized case numbers from aggregated case {caseId}", aggregatedCaseId);

        var result = await _next.Handle(aggregatedCaseId, cancellationToken);
        
        result.Switch(
            _ => _logger.LogInformation("{count} cases found in aggregated GetOrganized case {id}", result.Value.Count, aggregatedCaseId),
            errors => _logger.LogWarning("{name}: {errors}", nameof(GetAggregatedCaseHandler), errors.ToCommaDelimitedString()));

        return result;
    }
}