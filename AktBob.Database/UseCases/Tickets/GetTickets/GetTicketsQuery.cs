using AktBob.Database.Entities;
using Ardalis.Result;
using MassTransit.Mediator;

namespace AktBob.Database.UseCases.Tickets.GetTickets;
internal record GetTicketsQuery(int? DeskproId, long? PodioItemId, Guid? FilArkivCaseId, bool IncludeClosedTickets = true) : Request<Result<IEnumerable<Ticket>>>;
