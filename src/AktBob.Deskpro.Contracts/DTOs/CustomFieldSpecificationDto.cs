namespace AktBob.Deskpro.Contracts.DTOs;
public record CustomFieldSpecificationDto(int Id, string Title, IReadOnlyDictionary<int, string> Choices);