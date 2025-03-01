namespace AktBob.Database.Dtos;

public class MessageDto
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public int DeskproMessageId { get; set; }
    public int? GODocumentId { get; set; }
    public int? MessageNumber { get; set; }
}