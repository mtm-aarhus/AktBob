using System.Text.Json.Serialization;

namespace AktBob.CloudConvert.Models;
internal class MergeTask
{
    [JsonPropertyName("operation")]
    public string Operation { get; } = "merge";

    [JsonPropertyName("output_format")]
    public string OutputFormat { get; } = "pdf";

    [JsonPropertyName("engine")]
    public string Engine { get; } = "qpdf";

    [JsonPropertyName("input")]
    public string[] Input { get; set; } = Array.Empty<string>();
}
