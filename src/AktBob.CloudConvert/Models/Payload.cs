using System.Text.Json.Serialization;

namespace AktBob.CloudConvert.Models;
internal class Payload
{
    [JsonPropertyName("tasks")]
    public object Tasks { get; set; } = new();

    [JsonPropertyName("tag")]
    public string Tag { get; } = "AktBob.InternalWorker";
}
