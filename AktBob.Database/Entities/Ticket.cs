namespace AktBob.Database.Entities;
internal class Ticket
{
    public int Id { get; set; }
    public int DeskproId { get; set; }
    public string? CaseNumber { get; set; }
    public string? CaseUrl { get; set; }
    public string? SharepointFolderName { get; set; }
    public DateTime? TicketClosedAt { get; set; }
    public DateTime? JournalizedAt { get; set; }
    public List<Case> Cases { get; set; } = new List<Case>();
}
