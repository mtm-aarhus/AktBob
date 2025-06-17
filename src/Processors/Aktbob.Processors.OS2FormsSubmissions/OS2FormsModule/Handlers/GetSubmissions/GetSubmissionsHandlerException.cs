using ErrorOr;
using Microsoft.Extensions.Logging;

namespace Aktbob.Processors.OS2FormsSubmissions.OS2FormsModule.Handlers.GetSubmissions;

internal class GetSubmissionsHandlerException(IGetSubmissionsHandler next, ILogger<GetSubmissionsHandler> logger)
    : IGetSubmissionsHandler
{
    public async Task<ErrorOr<IReadOnlyCollection<Guid>>> Handle(string webformId, CancellationToken cancellationToken)
    {
        try
        {
            return await next.Handle(webformId, cancellationToken);
        }
        catch (HttpRequestException ex)
            when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return Error.NotFound("OS2Forms.SubmissionsNotFound", $"Submissions by webform {webformId} not found.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in {name}", nameof(GetSubmissionsHandler));
            throw;
        }
    }
}