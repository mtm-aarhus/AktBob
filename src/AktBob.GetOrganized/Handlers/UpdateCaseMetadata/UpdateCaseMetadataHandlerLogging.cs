using ErrorOr;
using Microsoft.Extensions.Logging;

namespace AktBob.GetOrganized.Handlers.UpdateCaseMetadata;

internal class UpdateCaseMetadataHandlerLogging(
    IUpdateCaseMetadataHandler next,
    ILogger<UpdateCaseMetadataHandler> logger)
    : IUpdateCaseMetadataHandler
{
    public async Task<ErrorOr<Success>> Handle(string caseId, Guid kleId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Update GetOrganized case {id} metadata: Kle = {kle}", caseId, kleId);

        var result = await next.Handle(caseId, kleId, cancellationToken);

        result.Switch(
            _ => logger.LogInformation("GetOrganized case {id} metadata updated", caseId),
            errors => logger.LogError("Error updating GetOrganized case {id} metadata", caseId));

        return result;
    }
}