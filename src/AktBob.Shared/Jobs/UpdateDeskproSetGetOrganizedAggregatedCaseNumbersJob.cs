namespace AktBob.Shared.Jobs;
public record UpdateDeskproSetGetOrganizedAggregatedCaseNumbersJob(string[] AggregatedCaseIds, int DeskproTicketId);