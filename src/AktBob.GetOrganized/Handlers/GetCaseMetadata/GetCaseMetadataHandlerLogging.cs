using AktBob.GetOrganized.Contracts.DTOs;
using AktBob.Shared.Extensions;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace AktBob.GetOrganized.Handlers.GetCaseMetadata;

internal class GetCaseMetadataHandlerLogging(IGetCaseMetadataHandler next, ILogger<GetCaseMetadataHandler> logger)
    : IGetCaseMetadataHandler
{
    public async Task<ErrorOr<CaseMetadataDto>> Handle(string caseId, CancellationToken cancellation = default)
    {
        logger.LogInformation("Getting GetOrganized case {id} metadata", caseId);

        var result = await next.Handle(caseId, cancellation);

        result.Switch(
            _ => logger.LogInformation("GetOrganized case {id} metadata retrieved", caseId),
            errors => logger.LogWarning("Error getting GetOrganized case {id} metadata: {errors}", caseId, errors.ToCommaDelimitedString()));

        return result;
    }
}