using ErrorOr;
using Microsoft.Extensions.Logging;

namespace AktBob.GetOrganized.Handlers.GetAggregatedCase;

internal class GetAggregatedCaseHandlerException(
    IGetAggregatedCaseHandler next,
    ILogger<GetAggregatedCaseHandler> logger)
    : IGetAggregatedCaseHandler
{
    public async Task<ErrorOr<IReadOnlyCollection<string>>> Handle(string aggregatedCaseId, CancellationToken cancellationToken)
    {
        try
        {
            return await next.Handle(aggregatedCaseId, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in {name}", nameof(GetAggregatedCaseHandler));
            return Error.Failure("GetOrganized.GetAggregatedCaseFailure", $"Failed to get aggregated case for {aggregatedCaseId}"); 
        }
    }
}