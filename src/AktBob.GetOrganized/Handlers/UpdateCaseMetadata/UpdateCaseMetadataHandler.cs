using AAK.GetOrganized;
using ErrorOr;
using Result = ErrorOr.Result;

namespace AktBob.GetOrganized.Handlers.UpdateCaseMetadata;
internal class UpdateCaseMetadataHandler : IUpdateCaseMetadataHandler
{
    private readonly IGetOrganizedClient _getOrganized;

    public UpdateCaseMetadataHandler(IGetOrganizedClient getOrganized)
    {
        _getOrganized = getOrganized;
    }
    
    public async Task<ErrorOr<Success>> Handle(string caseId, Guid kleId, CancellationToken cancellationToken = default)
    {
        await _getOrganized.UpdateCaseMetadata(caseId, new() { KleId = kleId }, cancellationToken);
        return Result.Success;
    }
}
