using AktBob.Shared.Types.Deskpro;

namespace Aktbob.Modules.Deskpro.Contracts.DTOs;
public class AttachmentDto
{
    public int Id { get; set; }
    public int BlobId { get; set; }
    public MessageId MessageId { get; set; }
    public int PersonId { get; set; }
    public bool IsAgentNote { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}
