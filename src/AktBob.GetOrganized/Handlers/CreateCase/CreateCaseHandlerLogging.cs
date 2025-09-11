using AktBob.GetOrganized.Contracts.DTOs;
using AktBob.Shared.Extensions;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace AktBob.GetOrganized.Handlers.CreateCase;

internal class CreateCaseHandlerLogging(ICreateCaseHandler next, ILogger<CreateCaseHandler> logger) : ICreateCaseHandler
{
    public async Task<ErrorOr<CreateCaseResponse>> Handle(
        string caseTitle,
        string caseProfile,
        string status,
        string access,
        string department,
        string facet,
        string kle,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating GetOrganized case: {title}", caseTitle);

        var result = await next.Handle(
            caseTitle,
            caseProfile,
            status,
            access,
            department,
            facet,
            kle,
            cancellationToken);
        
        result.Switch(
            _ => logger.LogInformation("GetOrganized case {caseId} created ({caseTitle})", result.Value.CaseId, caseTitle),
            errors =>  logger.LogDebug("{name}: {errors}", nameof(CreateCase), errors.ToCommaDelimitedString()));

        return result;
    }
}