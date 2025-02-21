namespace AktBob.Database.Contracts.Dtos;

public class MessageDto
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public int DeskproTicketId { get; set; }
    public int DeskproMessageId { get; set; }
    public int? GODocumentId { get; set; }
    public string? GOCaseNumber { get; set; }
    public string? Hash { get; set; }
    public DateTime? QueuedForJournalizationAt { get; set; }
    public int? MessageNumber { get; set; }
}