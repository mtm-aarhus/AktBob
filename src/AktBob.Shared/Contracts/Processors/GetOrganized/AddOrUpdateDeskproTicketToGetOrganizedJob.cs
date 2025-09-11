namespace AktBob.Shared.Contracts.Processors.GetOrganized;
public record AddOrUpdateDeskproTicketToGetOrganizedJob
{
    public DateTime SubmittedAt { get; } = DateTime.UtcNow;
    public int TicketId { get; init; }
    public string GOCaseNumber { get; init; } = string.Empty;
    public int[] CustomFieldIds { get; init; } = [];
    public int[] CaseNumberFieldIds { get; init; } = [];
}
