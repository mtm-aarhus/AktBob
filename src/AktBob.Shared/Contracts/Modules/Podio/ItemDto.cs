namespace AktBob.Shared.Contracts.Modules.Podio;

public record ItemDto(long Id, int AppId, IReadOnlyCollection<FieldDto> Fields);