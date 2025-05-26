using AktBob.GetOrganized.Contracts.DTOs;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace AktBob.GetOrganized.Handlers.GetCaseMetadata;

internal class GetCaseMetadataHandlerException : IGetCaseMetadataHandler
{
    private readonly IGetCaseMetadataHandler _next;
    private readonly ILogger<GetCaseMetadataHandler> _logger;

    public GetCaseMetadataHandlerException(IGetCaseMetadataHandler next, ILogger<GetCaseMetadataHandler> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task<ErrorOr<CaseMetadataDto>> Handle(string caseId, CancellationToken cancellation = default)
    {
        try
        {
            return await _next.Handle(caseId, cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(GetCaseMetadata));
            return Error.Failure("GetOrganized.GetCaseMetadataHandler", $"Failed to retrieve metadata for case {caseId}");
        }
    }
}