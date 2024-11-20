namespace AktBob.JournalizeDocuments;
internal record JournalizeMessageDto
{
    public string Subject { get; set; } = string.Empty;
    public int MessageId { get; set; }
    public int MessageNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public string MessageContent { get; set; } = string.Empty;
    public string PersonFromName { get; set; } = string.Empty;
    public string PersonFromEmail { get; set; } = string.Empty;
    public string[] AttachmentFileNames { get; set; } = [];
}
