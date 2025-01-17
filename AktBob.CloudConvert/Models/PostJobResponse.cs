using System.Text.Json.Serialization;

namespace AktBob.CloudConvert.Models;
internal class PostJobResponse
{
    [JsonPropertyName("data")]
    public PostJobResponseData Data { get; set; } = new();
}

internal class PostJobResponseData
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("tag")]
    public string? Tag { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("started_at")]
    public DateTime? StartedAt { get; set; }

    [JsonPropertyName("ended_at")]
    public DateTime? EndedAt { get; set; }
}
