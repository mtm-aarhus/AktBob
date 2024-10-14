namespace AktBob.DatabaseAPI.Contracts.DTOs;
public record CaseDto
{
    public int Id { get; set; }
    public long PodioItemId { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public Guid? FilArkivCaseId { get; set; }
    public string? SharepointFolderName { get; set; }
}