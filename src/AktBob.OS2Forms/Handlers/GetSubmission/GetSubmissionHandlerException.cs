using AktBob.OS2Forms.Contracts;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace AktBob.OS2Forms.Handlers.GetSubmission;

internal class GetSubmissionHandlerException : IGetSubmissionHandler
{
    private readonly IGetSubmissionHandler _next;
    private readonly ILogger<GetSubmissionHandler> _logger;

    public GetSubmissionHandlerException(IGetSubmissionHandler next, ILogger<GetSubmissionHandler> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task<ErrorOr<SubmissionDto>> Handle(Guid submissionId, string webformId, CancellationToken cancellationToken)
    {
        try
        {
            return await _next.Handle(submissionId, webformId, cancellationToken);
        }
        catch (HttpRequestException ex)
        when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return Error.NotFound("OS2Forms.SubmissionNotFound", $"Submission with ID {submissionId} not found.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(GetSubmissionHandler));
            throw;
        }
    }
}