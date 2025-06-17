using AktBob.Shared.Extensions;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace Aktbob.Processors.OS2FormsSubmissions.OS2FormsModule.Handlers.GetSubmissions;

internal class GetSubmissionsHandlerLogging(IGetSubmissionsHandler next, ILogger<GetSubmissionsHandler> logger)
    : IGetSubmissionsHandler
{
    public async Task<ErrorOr<IReadOnlyCollection<Guid>>> Handle(string webformId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting submissions from webform {webformId}", webformId);
        
        var result = await next.Handle(webformId, cancellationToken);
        
        result.Switch(
            _ => logger.LogInformation("Webform {webformId} submissions retrived", webformId),
            errors => logger.LogWarning("{name}: {errors}", nameof(GetSubmissionsHandler), errors.ToCommaDelimitedString()));

        return result;
    }
}