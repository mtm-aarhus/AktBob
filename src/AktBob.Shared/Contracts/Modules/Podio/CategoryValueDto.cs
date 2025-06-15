namespace AktBob.Shared.Contracts.Modules.Podio;

public record CategoryValueDto(int OptionId, string Value) : IFieldValueDto;