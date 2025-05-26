using ErrorOr;

namespace AktBob.GetOrganized.Handlers.GetAggregatedCase;

internal interface IGetAggregatedCaseHandler
{
    Task<ErrorOr<IReadOnlyCollection<string>>> Handle(string aggregatedCaseId, CancellationToken cancellationToken);
}
