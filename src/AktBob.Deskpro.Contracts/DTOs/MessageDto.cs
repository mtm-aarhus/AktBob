namespace AktBob.Deskpro.Contracts.DTOs;

public record MessageDto
{
    public int Id { get; init; }
    public int TicketId { get; init; }
    public PersonDto Person { get; set; } = new();
    public DateTime CreatedAt { get; init; }
    public bool IsAgentNote { get; init; }
    public string Content { get; init; } = string.Empty;
    public IEnumerable<int> AttachmentIds { get; set; } = Enumerable.Empty<int>();
    public IEnumerable<string> Recipients { get; set; } = Enumerable.Empty<string>();
}
