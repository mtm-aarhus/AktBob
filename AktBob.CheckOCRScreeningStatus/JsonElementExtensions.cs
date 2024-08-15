//using System.Text.Json;

//namespace AktBob.CheckOCRScreeningStatus;
//internal static class JsonElementExtensions
//{
//    private static readonly JsonSerializerOptions options = new()
//    {
//        PropertyNameCaseInsensitive = true,
//        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
//    };

//    public static T? TryGetValue<T>(this JsonElement element, string propertyName)
//    {
//        if (element.ValueKind != JsonValueKind.Object)
//        {
//            return default;
//        }

//        element.TryGetProperty(propertyName, out JsonElement property);

//        if (property.ValueKind == JsonValueKind.Undefined ||
//            property.ValueKind == JsonValueKind.Null)
//        {
//            return default;
//        }

//        try
//        {
//            return property.Deserialize<T>(options);
//        }
//        catch
//        {
//            return default;
//        }
//    }
//}
