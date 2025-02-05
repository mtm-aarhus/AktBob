namespace AktBob.Database.Endpoints.Cases.Post;
internal record PostCaseRequest
{
    public int TicketId { get; set; }
    public long PodioItemId { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public Guid? FilArkivCaseId { get; set; }
}
