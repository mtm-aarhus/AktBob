namespace AktBob.Database.Entities;

internal class Case
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public long PodioItemId { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public string? SharepointFolderName { get; set; }
    public Guid? FilArkivCaseId { get; set; }
}