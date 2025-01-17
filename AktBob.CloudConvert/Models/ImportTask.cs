using System.Text.Json.Serialization;

namespace AktBob.CloudConvert.Models;
internal class ImportTask
{
    [JsonPropertyName("operation")]
    public string Operation { get; } = "import/base64";
    
    [JsonPropertyName("file")]
    public required string File { get; set; }
    
    [JsonPropertyName("filename")]
    public required string Filename { get; set; }
}
