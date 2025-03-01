namespace AktBob.Database.Dtos;
public record TicketDto
{
    public int Id { get; set; }
    public int DeskproId { get; set; }
    public string? CaseNumber { get; set; }
    public string? CaseUrl { get; set; }
    public string? SharepointFolderName { get; set; }
    public IEnumerable<CaseDto> Cases { get; set; } = Enumerable.Empty<CaseDto>();
}
