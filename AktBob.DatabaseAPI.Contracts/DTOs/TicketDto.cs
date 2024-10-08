namespace AktBob.DatabaseAPI.Contracts.DTOs;
public record TicketDto
{
    public int Id { get; set; }
    public int DeskproId { get; set; }
    public string? CaseNumber { get; set; } = string.Empty;
    public string? FolderNameAktindsigter { get; set; }
    public string? FolderNameDocumentLists { get; set; }
    public IEnumerable<CaseDto>? Cases { get; set; } = new List<CaseDto>();
}