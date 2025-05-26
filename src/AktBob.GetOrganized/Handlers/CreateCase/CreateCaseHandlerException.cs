using AktBob.GetOrganized.Contracts.DTOs;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace AktBob.GetOrganized.Handlers.CreateCase;

internal class CreateCaseHandlerException : ICreateCaseHandler
{
    private readonly ICreateCaseHandler _next;
    private readonly ILogger<CreateCaseHandler> _logger;

    public CreateCaseHandlerException(ICreateCaseHandler next, ILogger<CreateCaseHandler> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task<ErrorOr<CreateCaseResponse>> Handle(string caseTitle, string caseProfile, string status, string access, string department, string facet,
        string kle, CancellationToken cancellationToken)
    {
        try
        {
            return await _next.Handle(
                caseTitle,
                caseProfile,
                status,
                access,
                department,
                facet,
                kle,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(CreateCaseHandler));
            return Error.Failure("CreateCaseHandler.Failure", ex.Message);
        }
    }
}