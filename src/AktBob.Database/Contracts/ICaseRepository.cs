using AktBob.Database.Entities;

namespace AktBob.Database.Contracts;
public interface ICaseRepository
{
    Task<int> Add(Case @case);
    Task<Case?> Get(int id);
    Task<Case?> GetByTicketId(int ticketId);
    Task<Case?> GetByPodioItemId(long podioItemId);
    Task<IEnumerable<Case>> GetAll(long? PodioItemId, Guid? FilArkivCaseId);
    Task<int> Update(Case @case);
}