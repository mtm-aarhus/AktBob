using System.Text.Json.Serialization;

namespace AktBob.CloudConvert.Models.JobResponse;

public class JobResponseData
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

    [JsonPropertyName("tasks")]
    public JobResponseTask[] Tasks { get; set; } = Array.Empty<JobResponseTask>();
}
