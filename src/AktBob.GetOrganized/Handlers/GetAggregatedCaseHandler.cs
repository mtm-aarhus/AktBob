using AAK.GetOrganized;
using AktBob.GetOrganized.Contracts;

namespace AktBob.GetOrganized.Handlers;

internal class GetAggregatedCaseHandler(IGetOrganizedClient getOrganizedClient) : IGetAggregatedCaseHandler
{
    public async Task<string[]> Handle(string aggregatedCaseId, CancellationToken cancellationToken)
    {
        var cases = await getOrganizedClient.GetCasesFromAggregatedCaseId(aggregatedCaseId, cancellationToken);
        return cases.Select(c => c.CaseId).ToArray();
    }
}
