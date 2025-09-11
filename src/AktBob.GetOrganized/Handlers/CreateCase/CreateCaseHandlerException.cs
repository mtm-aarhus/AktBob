using AktBob.GetOrganized.Contracts.DTOs;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace AktBob.GetOrganized.Handlers.CreateCase;

internal class CreateCaseHandlerException(ICreateCaseHandler next, ILogger<CreateCaseHandler> logger)
    : ICreateCaseHandler
{
    public async Task<ErrorOr<CreateCaseResponse>> Handle(string caseTitle, string caseProfile, string status, string access, string department, string facet,
        string kle, CancellationToken cancellationToken)
    {
        try
        {
            return await next.Handle(
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
            logger.LogError(ex, "Error in {name}", nameof(CreateCaseHandler));
            return Error.Failure("CreateCaseHandler.Failure", ex.Message);
        }
    }
}