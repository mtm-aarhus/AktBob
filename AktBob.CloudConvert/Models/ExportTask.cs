using System.Text.Json.Serialization;

namespace AktBob.CloudConvert.Models;
internal class ExportTask
{
    [JsonPropertyName("operation")]
    public string Operation { get; } = "export/url";

    [JsonPropertyName("inline")]
    public bool Inline { get; } = false;

    [JsonPropertyName("archive_multiple_files")]
    public bool ArchiveMultipleFiles { get; } = false;

    [JsonPropertyName("input")]
    public string[] Input { get; set; } = Array.Empty<string>();
}
