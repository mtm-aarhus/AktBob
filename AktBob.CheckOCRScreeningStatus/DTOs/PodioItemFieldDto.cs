using System.Text.Json.Serialization;

namespace AktBob.CheckOCRScreeningStatus.DTOs;
internal record PodioItemFieldDto
{
    public long Id { get; set;  }

    public string ExternalId { get; set;  } = string.Empty;

    public string Label { get; set;  } = string.Empty;

    public string Type { get; set;  } = string.Empty;

    [JsonPropertyName("values")]
    //[JsonConverter(typeof(StringOrArrayConverter))]
    public IEnumerable<string> Value { get; set; } = new List<string>();
}
