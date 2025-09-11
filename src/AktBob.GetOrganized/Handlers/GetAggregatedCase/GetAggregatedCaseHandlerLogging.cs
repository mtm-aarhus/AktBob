using AktBob.Shared.Extensions;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace  AktBob.GetOrganized.Handlers.GetAggregatedCase;

internal class GetAggregatedCaseHandlerLogging(IGetAggregatedCaseHandler next, ILogger<GetAggregatedCaseHandler> logger)
    : IGetAggregatedCaseHandler
{
    public async Task<ErrorOr<IReadOnlyCollection<string>>> Handle(string aggregatedCaseId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting GetOrganized case numbers from aggregated case {caseId}", aggregatedCaseId);

        var result = await next.Handle(aggregatedCaseId, cancellationToken);
        
        result.Switch(
            _ => logger.LogInformation("{count} cases found in aggregated GetOrganized case {id}", result.Value.Count, aggregatedCaseId),
            errors => logger.LogWarning("{name}: {errors}", nameof(GetAggregatedCaseHandler), errors.ToCommaDelimitedString()));

        return result;
    }
}