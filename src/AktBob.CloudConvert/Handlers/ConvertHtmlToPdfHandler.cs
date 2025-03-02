using AktBob.CloudConvert.Models;

namespace AktBob.CloudConvert.Handlers;
internal class ConvertHtmlToPdfHandler(ICloudConvertClient cloudConvertClient) : IConvertHtmlToPdfHandler
{
    private readonly ICloudConvertClient _cloudConvertClient = cloudConvertClient;

    public async Task<Result<Guid>> Handle(IReadOnlyDictionary<Guid, object> tasks, CancellationToken cancellationToken)
    {
        if (!tasks.Any())
        {
            return Result.Error("No tasks was provided. Cannot invoke CloudConvert with empty payload.");
        }

        if (tasks.Any(x => x.Value == null))
        {
            return Result.Error("One or more values in tasks dictionary is null. Cannot invoke CloudConvert with empty tasks.");
        }

        var payload = new Payload
        {
            Tasks = tasks
        };

        // Invoke CloudConvert and return job id
        var result = await _cloudConvertClient.CreateJob(payload, cancellationToken);
        if (!result.IsSuccess)
        {
            return Result.Error($"Error invoking CloudConvert. Payload: {payload}");
        }

        return result;
    }
}