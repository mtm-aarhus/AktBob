using System.Collections.ObjectModel;

namespace AktBob.Shared.Contracts.Modules.FilArkiv.Contracts.DTOs;
public record DocumentDto
{
    public Guid Id { get; set; }
    public Guid CaseId { get; set; }
    public string Title { get; set; } = default!;
    public int DocumentNumber { get; set; }
    public DateTime? DocumentDate { get; set; }
    public IReadOnlyCollection<FileDto> Files { get; set; } = new Collection<FileDto>();
}