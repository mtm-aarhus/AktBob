namespace AktBob.DatabaseAPI.Contracts;
public record TicketDto
{
    public int Id { get; set; }
    public int DeskproId { get; set; }
    public string? GOAktindsigtssagsnummer { get; set; } = string.Empty;
    public IEnumerable<CaseDto>? Cases { get; set; } = new List<CaseDto>();
}