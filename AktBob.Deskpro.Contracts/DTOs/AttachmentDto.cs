namespace AktBob.Deskpro.Contracts.DTOs;
public class AttachmentDto
{
    public int Id { get; set; }
    public int BlobId { get; set; }
    public int MessageId { get; set; }
    public int TicketId { get; set; }
    public int PersonId { get; set; }
    public bool IsAgentNote { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}
