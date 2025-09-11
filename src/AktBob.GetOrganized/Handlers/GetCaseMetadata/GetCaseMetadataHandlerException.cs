using AktBob.GetOrganized.Contracts.DTOs;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace AktBob.GetOrganized.Handlers.GetCaseMetadata;

internal class GetCaseMetadataHandlerException(IGetCaseMetadataHandler next, ILogger<GetCaseMetadataHandler> logger)
    : IGetCaseMetadataHandler
{
    public async Task<ErrorOr<CaseMetadataDto>> Handle(string caseId, CancellationToken cancellation = default)
    {
        try
        {
            return await next.Handle(caseId, cancellation);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in {name}", nameof(GetCaseMetadataHandler));
            return Error.Failure("GetOrganized.GetCaseMetadataHandler", $"Failed to retrieve metadata for case {caseId}");
        }
    }
}