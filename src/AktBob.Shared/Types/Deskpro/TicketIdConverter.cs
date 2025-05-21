//using System.Text.Json;
//using System.Text.Json.Serialization;

//namespace AktBob.Shared.Types.Deskpro;

//public class TicketIdConverter : JsonConverter<TicketId>
//{
//    public override TicketId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
//    {
//        var intValue = reader.GetInt32();
//        return TicketId.Create(intValue);
//    }

//    public override void Write(Utf8JsonWriter writer, TicketId value, JsonSerializerOptions options)
//    {
//        writer.WriteNumberValue(value);
//    }
//}
