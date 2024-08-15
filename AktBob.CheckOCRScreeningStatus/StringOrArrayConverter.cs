using System.Text.Json;
using System.Text.Json.Serialization;

internal class StringOrArrayConverter : JsonConverter<IEnumerable<string>>
{

    public override IEnumerable<string> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Case 1: Handle null values
        if (reader.TokenType == JsonTokenType.Null)
        {
            return Enumerable.Empty<string>();
        }

        // Case 2: Handle arrays of strings
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            var stringList = new List<string>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    break;
                }

                if (reader.TokenType == JsonTokenType.String)
                {
                    stringList.Add(reader.GetString()!);
                }
                else if (reader.TokenType == JsonTokenType.StartObject)
                {
                    // Optional: Skip objects if encountered within an array
                    SkipObject(ref reader);
                }
                else
                {
                    throw new JsonException($"Unexpected token {reader.TokenType} when parsing a list of strings.");
                }
            }
            return stringList.Count > 0 ? stringList : Enumerable.Empty<string>(); ;
        }

        // Case 3: Handle objects (if `values` is an object, return null or an empty list)
        if (reader.TokenType == JsonTokenType.StartObject)
        {
            SkipObject(ref reader);  // Skip the entire object
            return Enumerable.Empty<string>();
        }

        throw new JsonException("Unexpected token type.");
    }

    public override void Write(Utf8JsonWriter writer, IEnumerable<string> value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartArray();

        foreach (var item in value)
        {
            writer.WriteStringValue(item);
        }

        writer.WriteEndArray();
    }

    

    private void SkipObject(ref Utf8JsonReader reader)
    {
        int depth = 0;
        do
        {
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                depth++;
            }
            else if (reader.TokenType == JsonTokenType.EndObject)
            {
                depth--;
            }
        } while (depth > 0 && reader.Read());
    }
}
