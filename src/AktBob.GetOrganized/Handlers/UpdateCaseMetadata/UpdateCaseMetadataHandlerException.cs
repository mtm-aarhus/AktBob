using ErrorOr;
using Microsoft.Extensions.Logging;

namespace  AktBob.GetOrganized.Handlers.UpdateCaseMetadata;

internal class UpdateCaseMetadataHandlerException(
    IUpdateCaseMetadataHandler next,
    ILogger<UpdateCaseMetadataHandler> logger)
    : IUpdateCaseMetadataHandler
{
    public async Task<ErrorOr<Success>> Handle(string caseId, Guid kleId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await next.Handle(caseId, kleId, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in {name}", nameof(UpdateCaseMetadataHandler));
            return Error.Failure("GetOrganized.UpdateCaseMetadataHandlerFailure", $"Failed to update case {caseId}: {ex.Message}");
        }
    }
}