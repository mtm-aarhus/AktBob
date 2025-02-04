using System.Text.Json.Serialization;

namespace AktBob.CloudConvert.Models.JobResponse;
public class JobResponseRoot
{
    [JsonPropertyName("data")]
    public JobResponseData Data { get; set; } = new();
}
