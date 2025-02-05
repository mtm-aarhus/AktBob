namespace AktBob.Database.Endpoints.Tickets.Patch;
internal record PatchTicketRequest
{
    public int Id { get; set; }
    public string? CaseNumber { get; set; }
    public string? SharepointFolderName { get; set; }
    public DateTime? TicketClosedAt { get; set; }
    public DateTime? JournalizedAt { get; set; }
}