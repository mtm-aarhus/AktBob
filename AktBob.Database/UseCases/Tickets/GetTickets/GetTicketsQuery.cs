using AktBob.Database.Entities;
using Ardalis.Result;
using MediatR;

namespace AktBob.Database.UseCases.Tickets.GetTickets;
internal record GetTicketsQuery(int? DeskproId, long? PodioItemId, Guid? FilArkivCaseId, bool IncludeClosedTickets = true) : IRequest<Result<IEnumerable<Ticket>>>;
