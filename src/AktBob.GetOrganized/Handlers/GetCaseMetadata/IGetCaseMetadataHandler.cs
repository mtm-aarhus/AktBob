using AktBob.GetOrganized.Contracts.DTOs;
using ErrorOr;

namespace AktBob.GetOrganized.Handlers.GetCaseMetadata;
internal interface IGetCaseMetadataHandler
{
    Task<ErrorOr<CaseMetadataDto>> Handle(string caseId, CancellationToken cancellation = default);
}
