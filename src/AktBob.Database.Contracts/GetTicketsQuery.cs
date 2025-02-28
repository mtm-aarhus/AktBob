namespace AktBob.Database.Contracts;
public record GetTicketsQuery(int? DeskproId, long? PodioItemId, Guid? FilArkivCaseId, bool IncludeClosedTickets = true) : IQuery<Result<IEnumerable<TicketDto>>>;