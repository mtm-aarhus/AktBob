using System.Text.Json.Serialization;

namespace AktBob.CloudConvert.Models.JobResponse;

public class JobResponseTask
{
    [JsonPropertyName("operation")]
    public string Operation { get; set; } = string.Empty;

    [JsonPropertyName("result")]
    public JobResponseResult Result { get; set; } = new();
}
