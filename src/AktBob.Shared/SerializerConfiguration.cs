using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AktBob.Shared;

public static class SerializerConfiguration
{
    public static JsonSerializerOptions SerializerOptions(bool caseInsensitive = true, JsonNamingPolicy? jsonNamingPolicy = null) => new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = caseInsensitive,
        PropertyNamingPolicy = jsonNamingPolicy,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = false
    };
}