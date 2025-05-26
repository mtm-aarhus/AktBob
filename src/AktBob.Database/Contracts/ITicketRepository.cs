using AktBob.Database.Entities;
using AktBob.Shared.Types.Deskpro;

namespace AktBob.Database.Contracts;
public interface ITicketRepository
{
    Task<bool> Add(Ticket ticket);
    Task<Ticket?> GetByDeskproTicketId(TicketId deskproTicketId);
    Task<Ticket?> GetByPodioItemId(long podioItemId);
    Task<Ticket?> Get(int id);
    Task<bool> Update(Ticket ticket);
    Task<IReadOnlyCollection<Ticket>> GetAll(TicketId DeskproId, long? PodioItemId, Guid? FilArkivCaseId);
}
