using ErrorOr;
using Microsoft.Extensions.Logging;

namespace AktBob.GetOrganized.Handlers.UpdateCaseMetadata;

internal class UpdateCaseMetadataHandlerLogging : IUpdateCaseMetadataHandler
{
    private readonly IUpdateCaseMetadataHandler _next;
    private readonly ILogger<UpdateCaseMetadataHandler> _logger;

    public UpdateCaseMetadataHandlerLogging(IUpdateCaseMetadataHandler next, ILogger<UpdateCaseMetadataHandler> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task<ErrorOr<Success>> Handle(string caseId, string kle, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Update GetOrganized case {id} metadata: Kle = {kle}", caseId, kle);

        var result = await _next.Handle(caseId, kle, cancellationToken);

        result.Switch(
            _ => _logger.LogInformation("GetOrganized case {id} metadata updated", caseId),
            errors => _logger.LogError("Error updating GetOrganized case {id} metadata", caseId));

        return result;
    }
}