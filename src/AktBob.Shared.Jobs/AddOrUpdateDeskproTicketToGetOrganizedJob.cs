using AktBob.Shared.Types.Deskpro;

namespace AktBob.Shared.Jobs;
public record AddOrUpdateDeskproTicketToGetOrganizedJob
{
    public DateTime SubmittedAt { get; } = DateTime.UtcNow;
    public TicketId TicketId { get; set; }
    public string GOCaseNumber { get; set; } = string.Empty;
    public int[] CustomFieldIds { get; set; } = Array.Empty<int>();
    public int[] CaseNumberFieldIds { get; set; } = Array.Empty<int>();
}
