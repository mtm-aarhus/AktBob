using AAK.GetOrganized;
using ErrorOr;
using Result = ErrorOr.Result;

namespace AktBob.GetOrganized.Handlers.UpdateCaseMetadata;
internal class UpdateCaseMetadataHandler(IGetOrganizedClient getOrganized) : IUpdateCaseMetadataHandler
{
    public async Task<ErrorOr<Success>> Handle(string caseId, Guid kleId, CancellationToken cancellationToken = default)
    {
        await getOrganized.UpdateCaseMetadata(caseId, new() { KleId = kleId }, cancellationToken);
        return Result.Success;
    }
}
