using AktBob.Deskpro.Contracts.DTOs;

namespace AktBob.Deskpro.Contracts;
public record GetDeskproTicketsByFieldSearchQuery(int[] Fields, string SearchValue) : IRequest<Result<IEnumerable<TicketDto>>>;