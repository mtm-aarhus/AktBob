using Aktbob.Processors.OS2FormsSubmissions.OS2FormsModule.Contracts;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace Aktbob.Processors.OS2FormsSubmissions.OS2FormsModule.Handlers.GetSubmission;

internal class GetSubmissionHandlerException(IGetSubmissionHandler next, ILogger<GetSubmissionHandler> logger)
    : IGetSubmissionHandler
{
    public async Task<ErrorOr<SubmissionDto>> Handle(Guid submissionId, string webformId, CancellationToken cancellationToken)
    {
        try
        {
            return await next.Handle(submissionId, webformId, cancellationToken);
        }
        catch (HttpRequestException ex)
        when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return Error.NotFound("OS2Forms.SubmissionNotFound", $"Submission with ID {submissionId} not found.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in {name}", nameof(GetSubmissionHandler));
            throw;
        }
    }
}