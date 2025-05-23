using AAK.GetOrganized;
using AktBob.GetOrganized.Contracts;
using ErrorOr;

namespace AktBob.GetOrganized.Handlers;
internal class UpdateCaseMetadataHandler : IUpdateCaseMetadata
{
    private readonly IGetOrganizedClient _getOrganized;

    public UpdateCaseMetadataHandler(IGetOrganizedClient getOrganized)
    {
        _getOrganized = getOrganized;
    }

    public async Task<ErrorOr<Success>> Handle(string caseId, UpdateCaseMetadataCommand command, CancellationToken cancellationToken = default)
    {
        await _getOrganized.UpdateCaseMetadata(caseId, new() { Kle = command.Kle }, cancellationToken);
        return Result.Success;
    }
}
