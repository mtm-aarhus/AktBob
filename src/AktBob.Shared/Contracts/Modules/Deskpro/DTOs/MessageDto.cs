namespace AktBob.Shared.Contracts.Modules.Deskpro.DTOs;

public record MessageDto
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public PersonDto Person { get; set; } = new();
    public DateTime CreatedAt { get; init; }
    public bool IsAgentNote { get; init; }
    public string Content { get; init; } = string.Empty;
    public IEnumerable<int> AttachmentIds { get; set; } = Enumerable.Empty<int>();
    public IEnumerable<string> Recipients { get; set; } = Enumerable.Empty<string>();
    public string CreationSystem { get; set; } = string.Empty;
}
