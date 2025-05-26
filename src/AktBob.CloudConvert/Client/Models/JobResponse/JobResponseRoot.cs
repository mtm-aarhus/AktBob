using System.Text.Json.Serialization;

namespace AktBob.CloudConvert.Client.Models.JobResponse;
internal class JobResponseRoot
{
    [JsonPropertyName("data")]
    public JobResponseData Data { get; set; } = new();
}
