//using Newtonsoft.Json;

//namespace AktBob.Shared.Types.Deskpro;

//// The converter is needed for Hangfire to work properly when serializing/deserializing job objects

//public class TicketIdConverter : JsonConverter<TicketId>
//{
//    public override TicketId ReadJson(JsonReader reader, Type objectType, TicketId existingValue, bool hasExistingValue, JsonSerializer serializer)
//    {
//        var intValue = reader.Value != null ? Convert.ToInt32(reader.Value) : 0;
//        return TicketId.Create(intValue);
//    }

//    public override void WriteJson(JsonWriter writer, TicketId value, JsonSerializer serializer)
//    {
//        writer.WriteValue(value.Value);
//    }
//}
