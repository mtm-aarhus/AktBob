namespace AktBob.JournalizeDocuments;
internal record JournalizeFullTicketQueueItemDto
{
    public int TicketId { get; set; }
    public string GOCaseNumber { get; set; } = string.Empty;
    public int[] CustomFieldIds { get; set; } = Array.Empty<int>();
    public int[] CaseNumberFieldIds { get; set; } = Array.Empty<int>();

}
