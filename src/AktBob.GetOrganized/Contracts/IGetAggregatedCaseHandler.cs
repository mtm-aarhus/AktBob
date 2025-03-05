namespace AktBob.GetOrganized.Contracts;

internal interface IGetAggregatedCaseHandler
{
    Task<string[]> Handle(string aggregatedCaseId, CancellationToken cancellationToken);
}
