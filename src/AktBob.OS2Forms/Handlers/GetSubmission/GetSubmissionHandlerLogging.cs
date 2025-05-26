using AktBob.OS2Forms.Contracts;
using AktBob.Shared.Extensions;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace AktBob.OS2Forms.Handlers.GetSubmission;

internal class GetSubmissionHandlerLogging : IGetSubmissionHandler
{
    private readonly IGetSubmissionHandler _next;
    private readonly ILogger<GetSubmissionHandler> _logger;

    public GetSubmissionHandlerLogging(IGetSubmissionHandler next, ILogger<GetSubmissionHandler> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task<ErrorOr<SubmissionDto>> Handle(Guid submissionId, string webformId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting webform {webformId} submission {id}", webformId, submissionId);
        
        var result = await _next.Handle(submissionId, webformId, cancellationToken);
        
        result.Switch(
            _ => _logger.LogInformation("Webform {webformId} submission {id} retrived", webformId, submissionId),
            errors => _logger.LogWarning("{name}: {errors}", nameof(GetSubmissionHandler), errors.ToCommaDelimitedString()));

        return result;
    }
}