using AAK.GetOrganized;
using AktBob.GetOrganized.Handlers.UpdateCaseMetadata;
using ErrorOr;
using Result = ErrorOr.Result;

namespace AktBob.GetOrganized.Handlers;
internal class UpdateCaseMetadataHandler : IUpdateCaseMetadataHandler
{
    private readonly IGetOrganizedClient _getOrganized;

    public UpdateCaseMetadataHandler(IGetOrganizedClient getOrganized)
    {
        _getOrganized = getOrganized;
    }

    public async Task<ErrorOr<Success>> Handle(string caseId, string kle, CancellationToken cancellationToken = default)
    {
        await _getOrganized.UpdateCaseMetadata(caseId, new() { Kle = kle }, cancellationToken);
        return Result.Success;
    }
}
