namespace AktBob.Database.Entities;
public class Message
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public int DeskproMessageId { get; set; }
    public int? GODocumentId { get; set; }
    public int? MessageNumber { get; set; }
} 