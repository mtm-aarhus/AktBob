namespace AktBob.FilArkiv.Contracts;
public record FileDto
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public string FileName { get; set; } = default!;
}