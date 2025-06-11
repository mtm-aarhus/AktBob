namespace AktBob.Shared.Contracts.Database;

public record CaseDto
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public long PodioItemId { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public Guid? FilArkivCaseId { get; set; }
    public string? SharepointFolderName { get; set; }
}