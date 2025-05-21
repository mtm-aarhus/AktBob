using AktBob.CloudConvert.Client;
using AktBob.CloudConvert.Client.Models;

namespace AktBob.CloudConvert.Handlers.ConvertHtmlToPdf;

internal class ConvertHtmlToPdfHandler(ICloudConvertClient cloudConvertClient) : IConvertHtmlToPdfHandler
{
    private readonly ICloudConvertClient _cloudConvertClient = cloudConvertClient;

    public async Task<ErrorOr<Guid>> Handle(IReadOnlyDictionary<Guid, object> tasks, CancellationToken cancellationToken)
    {
        if (!tasks.Any())
        {
            return Error.Failure("CloudConvertConvertHtmlToPdfHandler.NoTasks", "No tasks was provided. Cannot invoke CloudConvert with empty payload.");
        }

        if (tasks.Any(x => x.Value == null))
        {
            return Error.Failure("CloudConvertConvertHtmlToPdfHandler.NullTasksNotAllowed", "One or more values in tasks dictionary is null. Cannot invoke CloudConvert with empty tasks.");
        }

        var payload = new Payload
        {
            Tasks = tasks
        };

        // Invoke CloudConvert and return job id
        return await _cloudConvertClient.CreateJob(payload, cancellationToken);
    }
}