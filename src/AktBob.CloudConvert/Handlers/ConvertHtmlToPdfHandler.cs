using AktBob.CloudConvert.Models;

namespace AktBob.CloudConvert.Handlers;
internal class ConvertHtmlToPdfHandler(ICloudConvertClient cloudConvertClient, ILogger<ConvertHtmlToPdfHandler> logger) : IConvertHtmlToPdfHandler
{
    private readonly ICloudConvertClient _cloudConvertClient = cloudConvertClient;
    private readonly ILogger<ConvertHtmlToPdfHandler> _logger = logger;

    public async Task<Result<Guid>> Handle(Dictionary<Guid, object> tasks, CancellationToken cancellationToken)
    {
        if (!tasks.Any())
        {
            _logger.LogError("No tasks was provided. Cannot invoke CloudConvert with empty payload");
            return Result.Error();
        }

        if (tasks.Any(x => x.Value == null))
        {
            _logger.LogError("One or more values in tasks dictionary is null. Cannot invoke CloudConvert with empty tasks");
            return Result.Error();
        }

        var payload = new Payload
        {
            Tasks = tasks
        };

        // Invoke CloudConvert and return job id
        var result = await _cloudConvertClient.CreateJob(payload, cancellationToken);
        if (!result.IsSuccess)
        {
            _logger.LogError("Error invoking CloudConvert. Payload: {payload}", payload);
        }

        return result;
    }
}
