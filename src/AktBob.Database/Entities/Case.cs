namespace AktBob.Database.Entities;

public class Case
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public long PodioItemId { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public Guid? FilArkivCaseId { get; set; }
    public string? SharepointFolderName { get; set; }
}