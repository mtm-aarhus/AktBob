using ErrorOr;
using Microsoft.Extensions.Logging;

namespace AktBob.GetOrganized.Handlers.GetAggregatedCase;

internal class GetAggregatedCaseHandlerException : IGetAggregatedCaseHandler
{
    private readonly IGetAggregatedCaseHandler _next;
    private readonly ILogger<GetAggregatedCaseHandler> _logger;

    public GetAggregatedCaseHandlerException(IGetAggregatedCaseHandler next, ILogger<GetAggregatedCaseHandler> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task<ErrorOr<IReadOnlyCollection<string>>> Handle(string aggregatedCaseId, CancellationToken cancellationToken)
    {
        try
        {
            return await _next.Handle(aggregatedCaseId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(GetAggregatedCase));
            return Error.Failure("GetOrganized.GetAggregatedCaseFailure", $"Failed to get aggregated case for {aggregatedCaseId}"); 
        }
    }
}