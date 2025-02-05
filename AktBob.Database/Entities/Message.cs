namespace AktBob.Database.Entities;
internal class Message
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public int DeskproTicketId { get; set; }
    public int DeskproMessageId { get; set; }
    public string? GOCaseNumber { get; set; }
    public int? GODocumentId { get; set; }
    public string? Hash { get; set; }
    public DateTime? QueuedForJournalizationAt { get; set; }
    public int? MessageNumber { get; set; }
}