using AktBob.Shared.Extensions;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace AktBob.OS2Forms.Handlers.GetSubmissions;

internal class GetSubmissionsHandlerLogging : IGetSubmissionsHandler
{
    private readonly IGetSubmissionsHandler _next;
    private readonly ILogger<GetSubmissionsHandler> _logger;

    public GetSubmissionsHandlerLogging(IGetSubmissionsHandler next, ILogger<GetSubmissionsHandler> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task<ErrorOr<IReadOnlyCollection<Guid>>> Handle(string webformId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting submissions from webform {webformId}", webformId);
        
        var result = await _next.Handle(webformId, cancellationToken);
        
        result.Switch(
            _ => _logger.LogInformation("Webform {webformId} submissions retrived", webformId),
            errors => _logger.LogWarning("{name}: {errors}", nameof(GetSubmissionsHandler), errors.ToCommaDelimitedString()));

        return result;
    }
}