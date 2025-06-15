namespace AktBob.Shared.Contracts.Modules.Podio;

public interface IFieldValueDto;
public record CategoryValueDto(int OptionId, string Value) : IFieldValueDto;
public record DateTimeValueDto(DateTime? StartDate, DateTime? EndDate) : IFieldValueDto;
public record ContactValueDto(string? Name, string? Email) : IFieldValueDto;