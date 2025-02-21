using System.Text.Json.Serialization;

namespace AktBob.CloudConvert.Models.JobResponse;

public class JobResponseFiles
{
    [JsonPropertyName("filename")]
    public string Filename { get; set; } = string.Empty;

    [JsonPropertyName("size")]
    public int Size { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
}
