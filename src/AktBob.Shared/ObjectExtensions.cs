using System.Text.Json;

namespace AktBob.Shared;
public static class ObjectExtensions
{
    public static string ToJson(this object obj)
    {
        var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = false, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
        return json;
    }
}
