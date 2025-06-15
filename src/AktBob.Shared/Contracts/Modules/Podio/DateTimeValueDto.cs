namespace AktBob.Shared.Contracts.Modules.Podio;

public record DateTimeValueDto(DateTime? StartDate, DateTime? EndDate) : IFieldValueDto;