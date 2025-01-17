using System.Text.Json.Serialization;

namespace AktBob.CloudConvert.Models.JobResponse;

internal class JobResponseResult
{
    [JsonPropertyName("files")]
    public JobResponseFiles[] Files { get; set; } = Array.Empty<JobResponseFiles>();
}
