using System.Text.Json.Serialization;

namespace AktBob.CloudConvert.Client.Models.JobResponse;

internal class JobResponseFiles
{
    [JsonPropertyName("filename")]
    public string Filename { get; set; } = string.Empty;

    [JsonPropertyName("size")]
    public int Size { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
}
