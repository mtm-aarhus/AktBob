using AktBob.Shared.Types.Deskpro;

namespace AktBob.Shared.Jobs;
public record UpdateDeskproSetGetOrganizedAggregatedCaseNumbersJob(string[] AggregatedCaseIds, TicketId TicketId);