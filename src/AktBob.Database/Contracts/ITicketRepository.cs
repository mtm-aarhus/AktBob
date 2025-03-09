using AktBob.Database.Entities;

namespace AktBob.Database.Contracts;
public interface ITicketRepository
{
    Task<bool> Add(Ticket ticket);
    Task<Ticket?> GetByDeskproTicketId(int deskproTicketId);
    Task<Ticket?> GetByPodioItemId(long podioItemId);
    Task<Ticket?> Get(int id);
    Task<int> Update(Ticket ticket);
    Task<IEnumerable<Ticket>> GetAll(int? DeskproId, long? PodioItemId, Guid? FilArkivCaseId);
}
