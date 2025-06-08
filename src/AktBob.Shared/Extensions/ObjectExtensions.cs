using System.Text.Json;

namespace AktBob.Shared.Extensions;
public static class ObjectExtensions
{
    public static string ToJson(this object obj, JsonSerializerOptions? options = null)
    {
        var json = JsonSerializer.Serialize(obj, options);
        return json;
    }
}
