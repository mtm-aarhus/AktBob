namespace AktBob.GetOrganized.Contracts;

internal interface IGetAggregatedCaseHandler
{
    Task<IReadOnlyCollection<string>> Handle(string aggregatedCaseId, CancellationToken cancellationToken);
}
