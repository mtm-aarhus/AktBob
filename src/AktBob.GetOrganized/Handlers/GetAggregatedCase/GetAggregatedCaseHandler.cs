using AAK.GetOrganized;
using ErrorOr;

namespace AktBob.GetOrganized.Handlers.GetAggregatedCase;

internal class GetAggregatedCaseHandler(IGetOrganizedClient getOrganizedClient) : IGetAggregatedCaseHandler
{
    public async Task<ErrorOr<IReadOnlyCollection<string>>> Handle(string aggregatedCaseId, CancellationToken cancellationToken)
    {
        var cases = await getOrganizedClient.GetCasesFromAggregatedCaseId(aggregatedCaseId, cancellationToken);
        return cases.Select(c => c.CaseId).ToArray();
    }
}
