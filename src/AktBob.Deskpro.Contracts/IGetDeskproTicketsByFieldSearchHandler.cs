using AktBob.Deskpro.Contracts.DTOs;

namespace AktBob.Deskpro.Contracts;
public interface IGetDeskproTicketsByFieldSearchHandler
{
    Task<Result<IEnumerable<TicketDto>>> Handle(int[] fields, string searchValue, CancellationToken cancellationToken);
}