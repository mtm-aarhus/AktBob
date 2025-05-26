using AktBob.GetOrganized.Contracts.DTOs;
using AktBob.Shared.Extensions;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace AktBob.GetOrganized.Handlers.CreateCase;

internal class CreateCaseHandlerLogging : ICreateCaseHandler
{
    private readonly ICreateCaseHandler _next;
    private readonly ILogger<CreateCaseHandler> _logger;

    public CreateCaseHandlerLogging(ICreateCaseHandler next, ILogger<CreateCaseHandler> logger)
    {
        _next = next;
        _logger = logger;
    }
    
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
        _logger.LogInformation("Creating GetOrganized case: {title}", caseTitle);

        var result = await _next.Handle(
            caseTitle,
            caseProfile,
            status,
            access,
            department,
            facet,
            kle,
            cancellationToken);
        
        result.Switch(
            _ => _logger.LogInformation("GetOrganized case {caseId} created ({caseTitle})", result.Value.CaseId, caseTitle),
            errors =>  _logger.LogDebug("{name}: {errors}", nameof(CreateCase), errors.ToCommaDelimitedString()));

        return result;
    }
}