using Microsoft.Extensions.Logging;
using ErrorOr;

namespace AktBob.OS2Forms.Handlers.GetSubmissions;

internal class GetSubmissionsHandlerException : IGetSubmissionsHandler
{
    private readonly IGetSubmissionsHandler _next;
    private readonly ILogger<GetSubmissionsHandler> _logger;

    public GetSubmissionsHandlerException(IGetSubmissionsHandler next, ILogger<GetSubmissionsHandler> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task<ErrorOr<IReadOnlyCollection<Guid>>> Handle(string webformId, CancellationToken cancellationToken)
    {
        try
        {
            return await _next.Handle(webformId, cancellationToken);
        }
        catch (HttpRequestException ex)
            when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return Error.NotFound("OS2Forms.SubmissionsNotFound", $"Submissions by webform {webformId} not found.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(GetSubmissionsHandler));
            throw;
        }
    }
}