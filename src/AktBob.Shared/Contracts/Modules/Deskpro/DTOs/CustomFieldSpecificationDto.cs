namespace AktBob.Shared.Contracts.Modules.Deskpro.DTOs;
public record CustomFieldSpecificationDto(int Id, string Title, IReadOnlyDictionary<int, string> Choices);