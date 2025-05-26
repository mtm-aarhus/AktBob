using AAK.GetOrganized;
using AktBob.GetOrganized.Contracts.DTOs;
using ErrorOr;

namespace AktBob.GetOrganized.Handlers.GetCaseMetadata;
internal class GetCaseMetadataHandler : IGetCaseMetadataHandler
{
    private readonly IGetOrganizedClient _getOrganized;

    public GetCaseMetadataHandler(IGetOrganizedClient getOrganized)
    {
        _getOrganized = getOrganized;
    }

    public async Task<ErrorOr<CaseMetadataDto>> Handle(string caseId, CancellationToken cancellation = default)
    {
        var result = await _getOrganized.GetCaseMetadata(caseId, cancellation);
        return result == null
            ? Error.NotFound("GetOrganized.CaseNotFound", $"Case {caseId} not found.").ToErrorOr<CaseMetadataDto>()
            : new CaseMetadataDto(result.CaseId, result.Kle);
    }
}
