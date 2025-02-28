using AktBob.Deskpro.Contracts.DTOs;

namespace AktBob.Deskpro.Contracts;
public record GetDeskproTicketsByFieldSearchQuery(int[] Fields, string SearchValue) : IQuery<Result<IEnumerable<TicketDto>>>;