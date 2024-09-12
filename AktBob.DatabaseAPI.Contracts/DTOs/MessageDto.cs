namespace AktBob.DatabaseAPI.Contracts.DTOs;
public class MessageDto
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public int DeskproTicketId { get; set; }
    public int DeskproMessageId { get; set; }
    public int? GODocumentId { get; set; }
    public string? GOCaseNumber { get; set; }
}
