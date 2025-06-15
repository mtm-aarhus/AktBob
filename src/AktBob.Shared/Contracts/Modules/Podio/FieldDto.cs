namespace AktBob.Shared.Contracts.Modules.Podio;

public record FieldDto(
    int Id,
    string Label,
    object? Value);