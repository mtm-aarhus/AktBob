using System.Text.Json.Nodes;
using AktBob.Shared.Contracts.Modules.Podio;

namespace AktBob.Shared.Extensions;

public static class PodioFieldExtensions
{
    public static T? GetValue<T>(this IReadOnlyCollection<FieldDto> fields, int fieldId)
    {
        var fieldValue = JsonValue.Create(fields.FirstOrDefault(x => x.Id == fieldId)?.Value);
        if (fieldValue is not (JsonNode and JsonValue valueNode)) return default;
        return valueNode.TryGetValue<T>(out var value) ? value : default;
    }
}