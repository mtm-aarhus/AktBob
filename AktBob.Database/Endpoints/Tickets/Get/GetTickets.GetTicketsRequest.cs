namespace AktBob.Database.Endpoints.Tickets.Get;
internal record GetTicketsRequest(int? DeskproId, long? PodioItemId, Guid? FilArkivCaseId, bool IncludeClosedTickets = true);