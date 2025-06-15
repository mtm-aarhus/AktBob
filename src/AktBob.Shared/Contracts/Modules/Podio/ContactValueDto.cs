namespace AktBob.Shared.Contracts.Modules.Podio;

public record ContactValueDto(string? Name, string? Email) : IFieldValueDto;