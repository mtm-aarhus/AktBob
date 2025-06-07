using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aktbob.Modules.OpenOrchestrator.Client.DTOs;

internal record QueueItem
{
    [JsonPropertyName("queue_name")] public required string QueueName { get; set; }
    [JsonPropertyName("data")] public JsonDocument? Data { get; set; }
    [JsonPropertyName("reference")] public string Reference { get; set; } = string.Empty;
    [JsonPropertyName("created_by")] public string CreatedBy { get; } = Environment.MachineName;
}