using Aktbob.Processors.OS2FormsSubmissions.OS2FormsModule.Contracts;
using AktBob.Shared.Extensions;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace Aktbob.Processors.OS2FormsSubmissions.OS2FormsModule.Handlers.GetSubmission;

internal class GetSubmissionHandlerLogging(IGetSubmissionHandler next, ILogger<GetSubmissionHandler> logger)
    : IGetSubmissionHandler
{
    public async Task<ErrorOr<SubmissionDto>> Handle(Guid submissionId, string webformId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting webform {webformId} submission {id}", webformId, submissionId);
        
        var result = await next.Handle(submissionId, webformId, cancellationToken);
        
        result.Switch(
            _ => logger.LogInformation("Webform {webformId} submission {id} retrived", webformId, submissionId),
            errors => logger.LogWarning("{name}: {errors}", nameof(GetSubmissionHandler), errors.ToCommaDelimitedString()));

        return result;
    }
}