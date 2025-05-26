using ErrorOr;
using Microsoft.Extensions.Logging;

namespace  AktBob.GetOrganized.Handlers.UpdateCaseMetadata;

internal class UpdateCaseMetadataHandlerException : IUpdateCaseMetadataHandler
{
    private readonly IUpdateCaseMetadataHandler _next;
    private readonly ILogger<UpdateCaseMetadataHandler> _logger;

    public UpdateCaseMetadataHandlerException(IUpdateCaseMetadataHandler next, ILogger<UpdateCaseMetadataHandler> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task<ErrorOr<Success>> Handle(string caseId, string kle, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _next.Handle(caseId, kle, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(UpdateCaseMetadata));
            return Error.Failure("GetOrganized.UpdateCaseMetadataHandlerFailure", $"Failed to update case {caseId}: {ex.Message}");
        }
    }
}