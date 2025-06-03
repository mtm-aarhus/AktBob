using ErrorOr;

namespace AktBob.GetOrganized.Handlers.UpdateCaseMetadata;
internal interface IUpdateCaseMetadataHandler
{
    Task<ErrorOr<Success>> Handle(string caseId, Guid kleId, CancellationToken cancellationToken = default);
}
