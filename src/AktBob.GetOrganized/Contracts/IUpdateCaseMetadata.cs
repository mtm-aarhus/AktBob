using ErrorOr;

namespace AktBob.GetOrganized.Contracts;
internal interface IUpdateCaseMetadata
{
    Task<ErrorOr<Success>> Handle(string caseId, UpdateCaseMetadataCommand command, CancellationToken cancellationToken = default);
}
